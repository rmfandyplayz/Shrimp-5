using UnityEngine;
using Sh.UIContract;
using System.Collections.Generic;
using System.Collections;

// written by andy
// cache/lookup additions and the sourceId sync fix by Claude Opus 5
// the "master script" that manages smaller, more specialized ui scripts
// has an "event queue" that processes ui changes in order
public class BattleUIManager : MonoBehaviour, IBattleUI
{
    [Header("Worker Scripts (handlers)")]
    [SerializeField] private List<MonoBehaviour> handlerScripts;

    private List<IBattleEventHandler> handlers; // actual list of interfaces to be populated at runtime

    private Queue<BattleEvent> eventQueue = new();
    private bool isBusy = false; // subsequent events won't be processed if true
    private IBattleEventHandler runningHandler; // whoever's mid-animation, so we can skip them

    private Dictionary<string, UIShrimpState> shrimpStateCache = new(); // retrieve shrimp state cache via id

    // who's currently out on each side. tracked from SwitchingShrimp events, because nothing
    // in BattleSetupData tells us who starts active
    private string activePlayerId;
    private string activeEnemyId;



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

    /// <summary>
    /// Initializes the UI's memory of what's going on with the game.
    ///
    /// Note that it doesn't do things such as setting the scene up. It just populates the UI's
    /// memory of all the characters that exist in the battle, so it can be referenced.
    ///
    /// Chances are, this function will only be called by Owen's code.
    /// </summary>
    public void InitializeBattle(BattleSetupData setupData)
    {
        shrimpStateCache.Clear();
        activePlayerId = null;
        activeEnemyId = null;

        if (setupData.characterData == null)
        {
            Debug.LogWarning("[BattleUIManager] >> InitializeBattle got no character data.");
            return;
        }

        foreach (ShrimpState charData in setupData.characterData) // both teams, flat list
        {
            AddToCache(charData);
        }

        // nothing in the setup data says who's out first, so just take the first shrimp we
        // find on each side. a SwitchingShrimp event will correct this the moment one arrives
        activePlayerId = GetFirstIdOnSide(BattleSide.Player);
        activeEnemyId = GetFirstIdOnSide(BattleSide.Enemy);
    }

    /// <summary>
    /// Initializes a <c>UIShrimpState</c> appropriately, and adds it to the cache.
    /// Helper function for <c>InitializeBattle()</c>
    /// </summary>
    private void AddToCache(ShrimpState charData)
    {
        if (charData == null || charData.definition == null)
        {
            Debug.LogWarning("[BattleUIManager] >> got a shrimp with no definition. skipping!");
            return;
        }

        // heads up: instanceID isn't assigned anywhere in the logic yet, so this is null in
        // practice right now. warn instead of throwing so the rest of the ui still comes up
        if (string.IsNullOrEmpty(charData.instanceID))
        {
            Debug.LogWarning($"[BattleUIManager] >> '{charData.name}' has no instanceID. " +
                $"the ui can't tell shrimp apart without one, so it won't be cached.");
            return;
        }

        UIShrimpState uiState = new()
        {
            shrimpUniqueId = charData.instanceID,
            displayName = !string.IsNullOrEmpty(charData.definition.displayName)
                ? charData.definition.displayName
                : charData.name, // fall back to the gameobject name

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

        if (shrimpStateCache.ContainsKey(uiState.shrimpUniqueId))
        {
            Debug.LogWarning($"[BattleUIManager] >> two shrimp share the instanceID " +
                $"'{uiState.shrimpUniqueId}'. overwriting the first one.");
        }

        shrimpStateCache[uiState.shrimpUniqueId] = uiState;
    }


    // lookups for handlers  ========================================================================================================

    /// <summary>
    /// Grabs the UI's copy of a shrimp's state by id. Returns false if we've never heard of it
    /// (which happens a lot while instanceID is still unassigned on the logic side).
    /// </summary>
    public bool TryGetShrimp(string shrimpId, out UIShrimpState shrimp)
    {
        shrimp = null;

        if (string.IsNullOrEmpty(shrimpId))
            return false;

        return shrimpStateCache.TryGetValue(shrimpId, out shrimp);
    }

    /// <summary>
    /// The shrimp currently out on the given side, or null if we don't know yet.
    /// </summary>
    public UIShrimpState GetActiveShrimp(BattleSide side)
    {
        string id = GetActiveShrimpId(side);

        TryGetShrimp(id, out UIShrimpState shrimp);
        return shrimp;
    }

    /// <summary>
    /// Id of whoever's out on the given side. Null until a shrimp is known to be active.
    /// </summary>
    public string GetActiveShrimpId(BattleSide side)
    {
        return side == BattleSide.Player ? activePlayerId : activeEnemyId;
    }

    /// <summary>
    /// Everyone on a side, in cache order. Used to fill the switch buttons.
    /// </summary>
    public List<UIShrimpState> GetTeam(BattleSide side)
    {
        List<UIShrimpState> team = new();

        foreach (KeyValuePair<string, UIShrimpState> entry in shrimpStateCache)
        {
            if (BattleSideResolver.FromId(entry.Key) == side)
            {
                team.Add(entry.Value);
            }
        }

        return team;
    }

    /// <summary>
    /// Everyone on a side who isn't the one currently out. This is what the switch menu wants.
    /// </summary>
    public List<UIShrimpState> GetBenchedTeam(BattleSide side)
    {
        List<UIShrimpState> bench = new();
        string activeId = GetActiveShrimpId(side);

        foreach (UIShrimpState shrimp in GetTeam(side))
        {
            if (shrimp.shrimpUniqueId != activeId)
            {
                bench.Add(shrimp);
            }
        }

        return bench;
    }

    // best guess at who starts out, used only until the first SwitchingShrimp event corrects
    // it. BattleSetupData doesn't say who's active, so there's nothing better to go on
    private string GetFirstIdOnSide(BattleSide side)
    {
        foreach (KeyValuePair<string, UIShrimpState> entry in shrimpStateCache)
        {
            if (BattleSideResolver.FromId(entry.Key) == side)
                return entry.Key;
        }

        return null;
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

    /// <summary>
    /// Tells whichever handler is mid-animation to hurry up and finish.
    /// Called by <c>BattleUIInput</c> when the player mashes confirm.
    /// </summary>
    public void ForceSkipCurrent()
    {
        runningHandler?.ForceSkip();
    }

    /// <summary>
    /// Whether an event is mid-flight. While this is true the queue is paused, so it's also
    /// the signal that a confirm press should mean "skip" rather than anything else.
    /// </summary>
    public bool IsBusy()
    {
        return isBusy;
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
            runningHandler = handler;
            yield return handler.HandleEvent(currentEvent);
            runningHandler = null;
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
    //
    // note: it's sourceId, not targetId. every producer in the logic puts the affected
    // character in sourceId and nothing ever sets targetId (the wiki says the same).
    //
    // also worth knowing: statusEffects on UIShrimpState is a LIVE reference to the logic's
    // own list, and the logic resolves a whole turn in one frame while we're still animating
    // event #1. so that list is already at its end-of-turn state and we can't trust it to tell
    // us what changed -- handlers read the event payload for that, and only re-read the list
    // on a full refresh (like a switch in).
    private void SyncCacheWithEvent(BattleEvent evt)
    {
        // some events legitimately have no shrimp attached (BattleWon, ChoosingMove, sounds)
        if (string.IsNullOrEmpty(evt.sourceId))
            return;

        // track who's out even for shrimp we don't have cached
        switch (evt.eventType)
        {
            case BattleEventType.SwitchingShrimp:
                SetActiveShrimp(evt.sourceId);
                break;
        }

        if (!shrimpStateCache.TryGetValue(evt.sourceId, out UIShrimpState character))
            return;

        // update local cache based on what logic said happened
        switch (evt.eventType)
        {
            // finalValue is the authoritative hp after the change. delta is just for display
            case BattleEventType.TakeDamage:
            case BattleEventType.Heal:
                character.currentHP = evt.finalValue;

                // maxValue isn't populated today, but respect it if it ever is
                if (evt.maxValue > 0)
                {
                    character.maxHP = evt.maxValue;
                }
                break;

            case BattleEventType.CharacterDied:
                character.currentHP = 0;
                break;

            // statuses change what the shrimp's stats resolve to, so re-read them.
            // the live list already reflects the change by the time we get here
            case BattleEventType.StatusApplied:
            case BattleEventType.StatusRemoved:
            case BattleEventType.AbilityTriggered:
                break;

            // these carry no state change for the cache -- they're purely visual/audio.
            // listed explicitly so it's obvious they weren't just forgotten
            case BattleEventType.Attack:
            case BattleEventType.SwitchingShrimp:
            case BattleEventType.ChoosingMove:
            case BattleEventType.BattleWon:
            case BattleEventType.BattleLost:
            case BattleEventType.LogMessage:
            case BattleEventType.PlaySound:
            case BattleEventType.GenericEffect:
            case BattleEventType.CameraShake:
                break;
        }
    }

    // remembers who's out on whichever side this shrimp belongs to.
    // does nothing for an id we can't place, rather than guessing at a team
    private void SetActiveShrimp(string shrimpId)
    {
        BattleSide side = BattleSideResolver.FromId(shrimpId);

        if (side == BattleSide.Player)
        {
            activePlayerId = shrimpId;
        }
        else if (side == BattleSide.Enemy)
        {
            activeEnemyId = shrimpId;
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
