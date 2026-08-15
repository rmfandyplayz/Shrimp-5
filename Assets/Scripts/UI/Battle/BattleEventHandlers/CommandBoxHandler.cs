using Sh.UIContract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// written by andy, co-authored by Claude Opus 5
// handles all necessary changes to the command box
//
// owns the two events that are purely about the box at the bottom:
//   ChoosingMove -- hand control to the player and wait for them to pick something
//   LogMessage   -- just say a line
//
// combat/status/roster text is said by those handlers instead, since they need to sequence it
// against the info displays updating.
public class CommandBoxHandler : BattleEventHandlerBase
{
    [Header("minion refs")]
    [SerializeField] CommandBox commandBox;
    [SerializeField, Tooltip("the only script allowed to talk back to the game logic")]
    private BattleUIInput battleInput;

    protected override BattleEventType[] HandledTypes => new[]
    {
        BattleEventType.ChoosingMove,
        BattleEventType.LogMessage
    };

    // set by the tile callbacks when the player commits to something
    private string pendingActionId;
    private ActionType pendingActionType;
    private bool choiceMade;

    protected override IEnumerator Handle(BattleEvent evt)
    {
        switch (evt.eventType)
        {
            case BattleEventType.ChoosingMove:
                yield return HandleChoosingMove(evt);
                break;

            case BattleEventType.LogMessage:
                yield return HandleLogMessage(evt);
                break;
        }
    }

    /// <summary>
    /// Hands the command box over to the player and doesn't return until they've committed to
    /// a move or a switch.
    ///
    /// This is the one handler that blocks on input rather than on an animation, so
    /// <c>ForceSkip</c> deliberately can't cut it short -- there's no way to continue without
    /// an actual answer.
    /// </summary>
    IEnumerator HandleChoosingMove(BattleEvent evt)
    {
        if (commandBox == null)
        {
            Debug.LogWarning("[CommandBoxHandler] >> no commandBox assigned, can't ask the player anything.");
            yield break;
        }

        UIShrimpState activeShrimp = uiManager != null
            ? uiManager.GetActiveShrimp(BattleSide.Player)
            : null;

        List<UIShrimpState> bench = uiManager != null
            ? uiManager.GetBenchedTeam(BattleSide.Player)
            : null;

        // if there's nothing to pick we'd sit in the wait loop below forever, so bail out
        // loudly instead. usually means InitializeBattle never ran, or instanceID is unset
        // so we couldn't work out who's on the player's team
        if (CountMoves(activeShrimp) == 0 && (bench == null || bench.Count == 0))
        {
            Debug.LogError("[CommandBoxHandler] >> ChoosingMove but the player has no moves " +
                "and nobody to switch to. skipping so the event queue doesn't lock up.");
            yield break;
        }

        choiceMade = false;
        pendingActionId = null;

        commandBox.SetDialogueInstant(BattleTextBuilder.ChoosingMove(evt, activeShrimp));

        // fill both menus up front so the player can flip between them instantly
        commandBox.PopulateMoveTiles(
            activeShrimp != null ? activeShrimp.moveData : null,
            OnMovePicked);

        commandBox.PopulateShrimpTiles(bench, OnShrimpPicked);

        commandBox.SwitchCommandBoxDisplayMode(CommandBoxMode.MOVE_SELECT);

        // deliberately ignores isSkipping -- there's nothing to skip, we need an actual answer
        while (!choiceMade)
        {
            yield return null;
        }

        // get out of the selection menus BEFORE telling the logic anything. SelectAction runs
        // the entire turn synchronously and queues every event for it on this same stack, so
        // if the buttons were still live the player could fire a second turn off mid-queue
        commandBox.SwitchCommandBoxDisplayMode(CommandBoxMode.DIALOGUE_DISPLAY);

        if (battleInput != null)
        {
            battleInput.SubmitAction(pendingActionId, pendingActionType);
        }
        else
        {
            Debug.LogWarning("[CommandBoxHandler] >> no battleInput assigned, the player's " +
                "choice has nowhere to go.");
        }
    }

    /// <summary>
    /// Says a designer-written line. Reads <c>flavorText</c> first, then <c>strings[0]</c>.
    /// </summary>
    IEnumerator HandleLogMessage(BattleEvent evt)
    {
        if (commandBox == null)
            yield break;

        string message = !string.IsNullOrEmpty(evt.flavorText)
            ? evt.flavorText
            : GetFirstString(evt);

        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("[CommandBoxHandler] >> got a LogMessage with nothing to say.");
            yield break;
        }

        yield return WaitFor(done => commandBox.SetDialogue(message, done));
    }

    // handed to the move tiles at bind time. just records the answer -- the coroutine above
    // is what actually acts on it, so the ordering around SelectAction stays in one place
    private void OnMovePicked(string moveId)
    {
        pendingActionId = moveId;
        pendingActionType = ActionType.Attacking;
        choiceMade = true;
    }

    private void OnShrimpPicked(string shrimpInstanceId)
    {
        pendingActionId = shrimpInstanceId;
        pendingActionType = ActionType.Switching;
        choiceMade = true;
    }

    // a skip during dialogue should dump the rest of the line out
    protected override void OnForceSkip()
    {
        if (commandBox != null)
        {
            commandBox.SkipDialogue();
        }
    }

    // how many non-null moves a shrimp actually has. the moves array is fixed at 3 slots and
    // can have gaps in it
    private static int CountMoves(UIShrimpState shrimp)
    {
        if (shrimp == null || shrimp.moveData == null)
            return 0;

        int count = 0;

        foreach (MoveDefinition move in shrimp.moveData)
        {
            if (move != null)
                count++;
        }

        return count;
    }

    private static string GetFirstString(BattleEvent evt)
    {
        if (evt.strings == null || evt.strings.Count == 0)
            return null;

        return evt.strings[0];
    }
}
