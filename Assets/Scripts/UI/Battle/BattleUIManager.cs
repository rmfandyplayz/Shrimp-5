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
        // TODO: pass data to a setup script.
    }

    public void QueueEvent(BattleEvent gameEvent)
    {
        eventQueue.Enqueue(gameEvent);
    }


    // everything else below is ui update control loop 

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
