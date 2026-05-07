using UnityEngine;
using Sh.UIContract;
using System.Collections.Generic;
using System.Collections;

// written by andy
// the "master script" that manages smaller, more specialized ui scripts
// has an "event queue" that processes ui changes in order
public class BattleUIManager : MonoBehaviour, IBattleUI
{
    [Header("Worker Scripts (handlers)")]
    [SerializeField] private List<MonoBehaviour> handlerScripts;

    private List<IBattleEventHandler> handlers; // actual list of interfaces to be populated at runtime
    
    private Queue<BattleEvent> eventQueue = new();
    private bool isBusy = false; // subsequent events won't be processed if true

    private Dictionary<string, UIShrimpState> shrimpStateCache = new(); // retrieve shrimp state cache via id



    private void Awake()
    {
        // find all the handlers (mini scripts for each ui element that actually does the
        // updating) and adds them to a list to keep track
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

    // logic code (owen's stuf) will call this to init all ui
    public void InitializeBattle(BattleSetupData setupData)
    {
        shrimpStateCache.Clear();

        foreach (ShrimpState charData in setupData.characterData) // for player team
        {
            AddToCache(charData);
        }
    }

    // helper function for InitializeBattle() to make it less cluttered
    // inits a UIShrimpState appropriately and adds it to the cache
    private void AddToCache(ShrimpState charData)
    {
        UIShrimpState uiState = new()
        {
            shrimpUniqueId = charData.instanceID,
            displayName = charData.name,

            spriteId = charData.definition.shrimpSpriteID,
            pfpId = charData.definition.pfpID,

            currentHP = charData.GetHP(),
            maxHP = charData.definition.maxHP,

            speed = charData.GetSpeed(),
            attack = charData.GetAttack(),

            ability = charData.definition.ability,
            moveData = charData.definition.moves,
            statusEffects = charData.statuses
        };

        shrimpStateCache.Add(uiState.shrimpUniqueId, uiState);
    }


    // everything else below is ui update control loop  ============================================================================================

    private void Update()
    {
        if (!isBusy && eventQueue.Count > 0)
        {
            StartCoroutine(ProcessNextEvent());
        }
    }

    public void QueueEvent(BattleEvent gameEvent) // called by logic
    {
        eventQueue.Enqueue(gameEvent);
    }

    // finds the first handler within the handlers list that can take care of
    // the current BattleEvent at hand, and waits for the handler to finish
    // before proceeding to the next one
    IEnumerator ProcessNextEvent()
    {
        isBusy = true;
        BattleEvent currentEvent = eventQueue.Dequeue();
        SyncCacheWithEvent(currentEvent);

        IBattleEventHandler handler = null;
        foreach (IBattleEventHandler h in handlers)
        {
            if (h.CanHandle(currentEvent.eventType))
            {
                handler = h;
                break; // found
            }
        }

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

    // update the UI's version of "truth model" before animations can play (so animations
    // can play correctly as they rely on the ui's stored data)
    // when an event is dequeued, it's passed to this function first to update the numbers
    private void SyncCacheWithEvent(BattleEvent evt)
    {
        // do nothing if there isn't a target or it aint in the dictionary
        if (string.IsNullOrEmpty(evt.targetId) || !shrimpStateCache.ContainsKey(evt.targetId))
            return;

        UIShrimpState character = shrimpStateCache[evt.targetId];

        // update local cache based on what logic said happened
        switch(evt.eventType)
        {
            case BattleEventType.TakeDamage:
            case BattleEventType.Heal:
                character.currentHP = evt.finalValue;
                break;

            // TODO: handling for all event types
        }
    }
}


[System.Serializable]
public class UIShrimpState
{
    public string shrimpUniqueId;
    public string displayName;

    public string spriteId;
    public string pfpId;

    public int currentHP;
    public int maxHP;

    public int speed;
    public int attack;

    public AbilityDefinition ability;
    public MoveDefinition[] moveData;
    public List<AppliedStatus> statusEffects;
}