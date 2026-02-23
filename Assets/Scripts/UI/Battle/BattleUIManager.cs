using UnityEngine;
using Sh.UIContract;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class BattleUIManager : MonoBehaviour, IBattleUI
{
    [Header("Worker Scripts (handlers)")]
    [SerializeField] private List<MonoBehaviour> handlerScripts;

    private List<IBattleEventHandler> handlers; // actual list of interfaces to be populated at runtime

    private Queue<BattleEvent> eventQueue = new();
    private bool isBusy = false; // subsequent events won't be processed if true

    private Dictionary<string, UIShrimpState> playerShrimpStateCache = new(); // retrieve shrimp state cache via id
    private Dictionary<string, UIShrimpState> enemyShrimpStateCache = new();



    private void Awake()
    {
        handlers = new List<IBattleEventHandler>();
        foreach (MonoBehaviour script in handlerScripts)
        {
            if (script is IBattleEventHandler handler)
            {
                handlers.Add(handler);
            }
            else
            {
                Debug.LogWarning($"[BattleUIManager] >> {script.name} does not implement IBattleEventHandler. skipping!");
            }
        }
    }

    // logic will call these two functions below.
    public void InitializeBattle(BattleSetupData setupData)
    {
        playerShrimpStateCache.Clear();

        foreach (var charData in setupData.playerTeam)
        {
            AddToCache(charData);
        }

        AddToCache(setupData.enemy, 1);
    }

    public void AddToCache(ShrimpDefinition data, int team = 0)
    {
        UIShrimpState state = new()
        {
            displayName = data.displayName,
            shrimpId = data.shrimpID,

            spriteId = data.shrimpSpriteID,
            //pfpId = data.pfpID,                                   TODO: owen add pfpId

            maxHP = data.maxHP,
            currentHP = data.maxHP,

            speed = data.baseSpeed,
            attack = data.baseAttack,

            ability = data.ability,
            moveData = data.moves,
            statusEffectIds = new()
        };

        if (team == 0)
            playerShrimpStateCache[data.shrimpID] = state;
        else
            enemyShrimpStateCache[data.shrimpID] = state;
    }


    // everything else below is ui update control loop 
    public void QueueEvent(BattleEvent gameEvent)
    {
        eventQueue.Enqueue(gameEvent);
    }

    private void Update()
    {
        if (!isBusy && eventQueue.Count > 0)
        {
            StartCoroutine(ProcessNextEvent());
        }
    }

    IEnumerator ProcessNextEvent()
    {
        isBusy = true;
        BattleEvent currentEvent = eventQueue.Dequeue();

        IBattleEventHandler handler = handlers.FirstOrDefault(h => h.CanHandle(currentEvent.eventType));

        if (handler != null) // get to work
        {
            yield return handler.HandleEvent(currentEvent);
        }
        else // where handler
        {
            Debug.LogWarning($"[BattleUIManager] >> no handler found for event type {currentEvent.eventType}");
            yield return null;
        }

        isBusy = false;
    }

    // helper method for process next event to sync 
    private void SyncCacheWithEvent(BattleEvent evt)
    {

    }
}


[System.Serializable]
public class UIShrimpState
{
    public string displayName;
    public string shrimpId;

    public string spriteId;
    public string pfpId;

    public int currentHP;
    public int maxHP;

    public int speed;
    public int attack;

    public AbilityDefinition ability;
    public MoveDefinition[] moveData;
    public List<string> statusEffectIds;
}