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

    private Dictionary<string, UIShrimpState> shrimpStateCache = new(); // retrieve shrimp state cache via id



    private void Awake()
    {
        handlers = new List<IBattleEventHandler>();
        foreach(MonoBehaviour script in handlerScripts)
        {
            if(script is IBattleEventHandler handler)
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
        shrimpStateCache.Clear();

        foreach(var charData in setupData.playerTeam)
        {
            
        }
        foreach(var charData in setupData.enemyTeam)
        {

        }
    }

    public void AddToCache(CharacterInitialData data)
    {
        // TODO: finish this func and initialize battle. inform owen he might have to make changes to his code since
        //       characterinitialdata will probably need a bit of rework
    }


    // everything else below is ui update control loop 
    public void QueueEvent(BattleEvent gameEvent)
    {
        eventQueue.Enqueue(gameEvent);
    }

    private void Update()
    {
        if(!isBusy && eventQueue.Count > 0)
        {
            StartCoroutine(ProcessNextEvent());
        }
    }

    IEnumerator ProcessNextEvent()
    {
        isBusy = true;
        BattleEvent currentEvent = eventQueue.Dequeue();

        IBattleEventHandler handler = handlers.FirstOrDefault(h => h.CanHandle(currentEvent.eventType));

        if(handler != null) // get to work
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
}


[System.Serializable]
public class UIShrimpState
{
    public string shrimpId;
    public int currentHP;


    public Sprite pfp;
    public List<string> moveIds;
    public List<string> abilityIds;
}