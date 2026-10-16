using Sh.UIContract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// written by Claude Opus 5
// auto-init, faked status bookkeeping and the target controls for BattleDebugPanel added by Claude Opus 5.5
// test rig for the battle ui. lets us drive every BattleEventType by hand without needing
// owen's side to work yet.
//
// how to use: drop this in the scene, point it at the BattleUIManager, drag some ShrimpStates
// into the two team lists and enter play mode. then either use the on-screen BattleDebugPanel
// buttons, or right click the component header and pick things off the menu.
//
// it also assigns instanceIDs, since the logic doesn't do that yet and the ui can't tell
// shrimp apart without them. delete that bit once owen's assigning them properly.
public class BattleUIDebugDriver : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private BattleUIManager uiManager;

    [Header("setup")]
    [SerializeField, Tooltip("initialize the fake battle by itself a frame after play starts")]
    private bool initializeOnStart = true;

    [Header("fake teams")]
    [SerializeField] private List<ShrimpState> playerTeam = new();
    [SerializeField] private List<ShrimpState> enemyTeam = new();

    [Header("fake event values")]
    [SerializeField] private int fakeDamage = 12;
    [SerializeField] private int fakeHeal = 8;
    [SerializeField, Tooltip("what StatusApplied puts on the target. the logic normally does " +
        "the adding, so the driver fakes that part too")]
    private StatusDefinition debugStatus;
    [SerializeField, Tooltip("which shrimp events get pointed at. 0 = first on the team")]
    private int targetIndex = 0;
    [SerializeField] private bool targetEnemy = false;

    private void Awake()
    {
        if (uiManager == null)
        {
            uiManager = GetComponent<BattleUIManager>();
        }
    }

    private IEnumerator Start()
    {
        if (!initializeOnStart)
            yield break;

        // wait a frame. ShrimpState.Start sets its hp and swaps in a fresh statuses list, and
        // the ui keeps a live reference to that list -- initializing before it runs would
        // leave the ui watching a list nothing ever writes to
        yield return null;

        InitializeFakeBattle();
    }

    /// <summary>
    /// Stamps instanceIDs onto the two team lists and pushes them through
    /// <c>InitializeBattle</c>. Run this before firing any events, or nothing will resolve.
    /// </summary>
    [ContextMenu("Setup/Initialize fake battle")]
    public void InitializeFakeBattle()
    {
        if (!HasManager())
            return;

        // TEMP: the logic never sets instanceID, so nothing in the ui can tell shrimp apart.
        // stamping the ids the contract's comments describe (shrimp.player.1 / shrimp.enemy.3)
        PrepareTeam(playerTeam, "shrimp.player.");
        PrepareTeam(enemyTeam, "shrimp.enemy.");

        List<ShrimpState> everyone = new();
        everyone.AddRange(playerTeam);
        everyone.AddRange(enemyTeam);

        BattleSetupData setup = new() { characterData = everyone };
        uiManager.InitializeBattle(setup);

        Debug.Log($"[BattleUIDebugDriver] >> initialized with {playerTeam.Count} player and " +
            $"{enemyTeam.Count} enemy shrimp.");
    }

    // stamps the ids and puts everyone back to a fresh battle (full hp, no statuses), so
    // initializing again mid-test actually starts over
    private static void PrepareTeam(List<ShrimpState> team, string prefix)
    {
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i] == null)
                continue;

            team[i].instanceID = prefix + (i + 1);

            // Start() normally does this, but it may not have run yet when we're poked
            // from the context menu
            if (team[i].statuses == null)
            {
                team[i].statuses = new List<AppliedStatus>();
            }

            team[i].statuses.Clear();

            if (team[i].definition != null)
            {
                team[i].currentHP = team[i].definition.maxHP;
            }
        }
    }


    // combat  ==============================================================================

    [ContextMenu("Events/Attack")]
    public void FireAttack()
    {
        UIShrimpState shrimp = GetTargetShrimp();

        Queue(new BattleEvent
        {
            eventType = BattleEventType.Attack,
            sourceId = GetTargetId(),
            moveId = GetFirstMoveId(shrimp)
        });
    }

    // hp lives on the ShrimpState, same as in the real logic. NOT read back off the ui's
    // cache -- that only catches up as each event drains, so two hits queued back to back
    // would both be worked out from the same starting hp

    [ContextMenu("Events/TakeDamage")]
    public void FireTakeDamage()
    {
        ShrimpState target = GetTargetState();
        int finalHP = 0;

        if (target != null)
        {
            target.currentHP = Mathf.Max(0, target.currentHP - fakeDamage);
            finalHP = target.currentHP;
        }

        Queue(new BattleEvent
        {
            eventType = BattleEventType.TakeDamage,
            sourceId = GetTargetId(),
            deltaValue = fakeDamage,
            finalValue = finalHP
        });
    }

    [ContextMenu("Events/Heal")]
    public void FireHeal()
    {
        ShrimpState target = GetTargetState();
        int finalHP = 0;

        if (target != null)
        {
            int maxHP = target.definition != null ? target.definition.maxHP : target.currentHP + fakeHeal;
            target.currentHP = Mathf.Min(maxHP, target.currentHP + fakeHeal);
            finalHP = target.currentHP;
        }

        Queue(new BattleEvent
        {
            eventType = BattleEventType.Heal,
            sourceId = GetTargetId(),
            deltaValue = fakeHeal,
            finalValue = finalHP
        });
    }


    // statuses  ============================================================================

    // the logic changes the shrimp's status list BEFORE it reports the change, and the ui
    // reads that list live. so these two do the same: edit the list, then send the event

    [ContextMenu("Events/StatusApplied")]
    public void FireStatusApplied()
    {
        ShrimpState target = GetTargetState();

        if (target != null && debugStatus != null)
        {
            target.statuses ??= new List<AppliedStatus>();
            target.statuses.Add(new AppliedStatus(debugStatus));
        }
        else
        {
            Debug.LogWarning("[BattleUIDebugDriver] >> no debugStatus or no target, so nothing " +
                "was put on the shrimp. the event still goes out but there's no icon to show.");
        }

        Queue(new BattleEvent
        {
            eventType = BattleEventType.StatusApplied,
            sourceId = GetTargetId(),
            strings = new List<string> { debugStatus != null ? debugStatus.statusID : "" }
        });
    }

    [ContextMenu("Events/StatusRemoved")]
    public void FireStatusRemoved()
    {
        ShrimpState target = GetTargetState();
        AppliedStatus removed = null;

        if (target != null && target.statuses != null && target.statuses.Count > 0)
        {
            removed = target.statuses[target.statuses.Count - 1];
            target.statuses.RemoveAt(target.statuses.Count - 1);
        }

        Queue(new BattleEvent
        {
            eventType = BattleEventType.StatusRemoved,
            sourceId = GetTargetId(),
            strings = new List<string> { removed != null && removed.status != null ? removed.status.statusID : "" }
        });
    }

    [ContextMenu("Events/AbilityTriggered")]
    public void FireAbilityTriggered()
    {
        UIShrimpState shrimp = GetTargetShrimp();
        string abilityId = shrimp != null && shrimp.ability != null ? shrimp.ability.abilityID : "";

        Queue(new BattleEvent
        {
            eventType = BattleEventType.AbilityTriggered,
            sourceId = GetTargetId(),
            strings = new List<string> { abilityId }
        });
    }


    // roster  ==============================================================================

    [ContextMenu("Events/SwitchingShrimp")]
    public void FireSwitchingShrimp()
    {
        // switch to whoever ISN'T out right now, so there's a visible change
        List<ShrimpState> team = targetEnemy ? enemyTeam : playerTeam;
        int next = team.Count > 1 ? (targetIndex + 1) % team.Count : targetIndex;

        Queue(new BattleEvent
        {
            eventType = BattleEventType.SwitchingShrimp,
            sourceId = GetIdAt(team, next)
        });

        // follow whoever just came out, so the next events hit the shrimp that's on screen
        // and pressing this again keeps cycling instead of switching to the same one
        targetIndex = next;
    }

    [ContextMenu("Events/CharacterDied")]
    public void FireCharacterDied()
    {
        ShrimpState target = GetTargetState();

        if (target != null)
        {
            target.currentHP = 0;
        }

        Queue(new BattleEvent
        {
            eventType = BattleEventType.CharacterDied,
            sourceId = GetTargetId(),
            finalValue = 0
        });
    }


    // flow  ================================================================================

    [ContextMenu("Events/ChoosingMove")]
    public void FireChoosingMove()
    {
        Queue(new BattleEvent { eventType = BattleEventType.ChoosingMove });
    }

    [ContextMenu("Events/BattleWon")]
    public void FireBattleWon()
    {
        Queue(new BattleEvent { eventType = BattleEventType.BattleWon });
    }

    [ContextMenu("Events/BattleLost")]
    public void FireBattleLost()
    {
        Queue(new BattleEvent { eventType = BattleEventType.BattleLost });
    }


    // junk drawer  =========================================================================

    [ContextMenu("Events/LogMessage")]
    public void FireLogMessage()
    {
        Queue(new BattleEvent
        {
            eventType = BattleEventType.LogMessage,
            flavorText = "the shrimp are getting restless..."
        });
    }

    [ContextMenu("Events/PlaySound")]
    public void FirePlaySound()
    {
        Queue(new BattleEvent
        {
            eventType = BattleEventType.PlaySound,
            strings = new List<string> { "bonk" }
        });
    }

    [ContextMenu("Events/GenericEffect")]
    public void FireGenericEffect()
    {
        Queue(new BattleEvent
        {
            eventType = BattleEventType.GenericEffect,
            strings = new List<string> { "debug.effect" }
        });
    }

    [ContextMenu("Events/CameraShake")]
    public void FireCameraShake()
    {
        Queue(new BattleEvent { eventType = BattleEventType.CameraShake });
    }


    /// <summary>
    /// Queues one of every event type back to back, in a rough approximation of turn order,
    /// so you can watch the whole thing drain and check nothing throws.
    ///
    /// Note that ChoosingMove blocks on real input, so this will sit and wait for you to pick
    /// something before the rest go through.
    /// </summary>
    [ContextMenu("Setup/Fire every event type")]
    public void FireEverything()
    {
        FireChoosingMove();
        FireAttack();
        FireTakeDamage();
        FireStatusApplied();
        FireAbilityTriggered();
        FireHeal();
        FireStatusRemoved();
        FireCameraShake();
        FirePlaySound();
        FireGenericEffect();
        FireLogMessage();
        FireCharacterDied();
        FireSwitchingShrimp();
        FireBattleWon();
    }


    // target controls  =====================================================================
    // so the on-screen debug panel can repoint events without opening the inspector

    [ContextMenu("Target/Toggle player or enemy")]
    public void ToggleTargetSide()
    {
        targetEnemy = !targetEnemy;

        List<ShrimpState> team = targetEnemy ? enemyTeam : playerTeam;
        targetIndex = Mathf.Clamp(targetIndex, 0, Mathf.Max(0, team.Count - 1));
    }

    [ContextMenu("Target/Next shrimp on team")]
    public void CycleTargetIndex()
    {
        List<ShrimpState> team = targetEnemy ? enemyTeam : playerTeam;
        targetIndex = team.Count > 0 ? (targetIndex + 1) % team.Count : 0;
    }

    /// <summary>
    /// Who events are currently pointed at, e.g. <c>enemy #2 (shrimp.enemy.2)</c>.
    /// </summary>
    public string GetTargetDescription()
    {
        string id = GetTargetId();
        return $"{(targetEnemy ? "enemy" : "player")} #{targetIndex + 1} ({id ?? "nobody"})";
    }


    // helpers  =============================================================================

    private void Queue(BattleEvent evt)
    {
        if (!HasManager())
            return;

        uiManager.QueueEvent(evt);
    }

    private bool HasManager()
    {
        if (uiManager != null)
            return true;

        Debug.LogWarning("[BattleUIDebugDriver] >> no BattleUIManager assigned.");
        return false;
    }

    // whichever shrimp the targetIndex / targetEnemy fields are currently pointing at
    private string GetTargetId()
    {
        return GetIdAt(targetEnemy ? enemyTeam : playerTeam, targetIndex);
    }

    private static string GetIdAt(List<ShrimpState> team, int index)
    {
        if (team == null || index < 0 || index >= team.Count || team[index] == null)
            return null;

        return team[index].instanceID;
    }

    private UIShrimpState GetTargetShrimp()
    {
        if (uiManager == null)
            return null;

        uiManager.TryGetShrimp(GetTargetId(), out UIShrimpState shrimp);
        return shrimp;
    }

    private static string GetFirstMoveId(UIShrimpState shrimp)
    {
        if (shrimp == null || shrimp.moveData == null)
            return null;

        foreach (MoveDefinition move in shrimp.moveData)
        {
            if (move != null)
                return move.moveID;
        }

        return null;
    }

    // the logic-side object behind the current target, for faking the logic's own bookkeeping
    private ShrimpState GetTargetState()
    {
        List<ShrimpState> team = targetEnemy ? enemyTeam : playerTeam;

        if (team == null || targetIndex < 0 || targetIndex >= team.Count)
            return null;

        return team[targetIndex];
    }
}
