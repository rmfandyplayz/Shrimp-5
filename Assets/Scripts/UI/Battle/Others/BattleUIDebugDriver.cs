using Sh.UIContract;
using System.Collections.Generic;
using UnityEngine;

// written by Claude Opus 5
// test rig for the battle ui. lets us drive every BattleEventType by hand without needing
// owen's side to work yet.
//
// how to use: drop this next to the BattleUIManager, drag some ShrimpStates into the two team
// lists, enter play mode, then right click the component header and pick things off the menu.
//
// it also assigns instanceIDs, since the logic doesn't do that yet and the ui can't tell
// shrimp apart without them. delete that bit once owen's assigning them properly.
public class BattleUIDebugDriver : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private BattleUIManager uiManager;

    [Header("fake teams")]
    [SerializeField] private List<ShrimpState> playerTeam = new();
    [SerializeField] private List<ShrimpState> enemyTeam = new();

    [Header("fake event values")]
    [SerializeField] private int fakeDamage = 12;
    [SerializeField] private int fakeHeal = 8;
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
        AssignInstanceIds(playerTeam, "shrimp.player.");
        AssignInstanceIds(enemyTeam, "shrimp.enemy.");

        List<ShrimpState> everyone = new();
        everyone.AddRange(playerTeam);
        everyone.AddRange(enemyTeam);

        BattleSetupData setup = new() { characterData = everyone };
        uiManager.InitializeBattle(setup);

        Debug.Log($"[BattleUIDebugDriver] >> initialized with {playerTeam.Count} player and " +
            $"{enemyTeam.Count} enemy shrimp.");
    }

    private static void AssignInstanceIds(List<ShrimpState> team, string prefix)
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

    [ContextMenu("Events/TakeDamage")]
    public void FireTakeDamage()
    {
        UIShrimpState shrimp = GetTargetShrimp();
        int currentHP = shrimp != null ? shrimp.currentHP : fakeDamage * 3;

        Queue(new BattleEvent
        {
            eventType = BattleEventType.TakeDamage,
            sourceId = GetTargetId(),
            deltaValue = fakeDamage,
            finalValue = Mathf.Max(0, currentHP - fakeDamage)
        });
    }

    [ContextMenu("Events/Heal")]
    public void FireHeal()
    {
        UIShrimpState shrimp = GetTargetShrimp();
        int currentHP = shrimp != null ? shrimp.currentHP : 0;
        int maxHP = shrimp != null ? shrimp.maxHP : fakeHeal * 4;

        Queue(new BattleEvent
        {
            eventType = BattleEventType.Heal,
            sourceId = GetTargetId(),
            deltaValue = fakeHeal,
            finalValue = Mathf.Min(maxHP, currentHP + fakeHeal)
        });
    }


    // statuses  ============================================================================

    [ContextMenu("Events/StatusApplied")]
    public void FireStatusApplied()
    {
        Queue(new BattleEvent
        {
            eventType = BattleEventType.StatusApplied,
            sourceId = GetTargetId(),
            strings = new List<string> { GetFirstStatusId() }
        });
    }

    [ContextMenu("Events/StatusRemoved")]
    public void FireStatusRemoved()
    {
        Queue(new BattleEvent
        {
            eventType = BattleEventType.StatusRemoved,
            sourceId = GetTargetId(),
            strings = new List<string> { GetFirstStatusId() }
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
    }

    [ContextMenu("Events/CharacterDied")]
    public void FireCharacterDied()
    {
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

    private string GetFirstStatusId()
    {
        UIShrimpState shrimp = GetTargetShrimp();

        if (shrimp == null || shrimp.statusEffects == null || shrimp.statusEffects.Count == 0)
            return "";

        AppliedStatus first = shrimp.statusEffects[0];
        return first != null && first.status != null ? first.status.statusID : "";
    }
}
