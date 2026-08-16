using Sh.UIContract;
using UnityEngine;

// written by andy
// input handling and the SelectAction submission path filled in by Claude Opus 5
// handles input and
// communicates with battle logic about what the player wants to do
//
// this is the ONLY script that's allowed to call into IBattleCommands. everything else that
// wants to tell the logic something goes through here, so there's one place to look when the
// handshake between ui and logic goes weird.
public class BattleUIInput : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private BattleUIManager uiManager;
    [SerializeField] private CommandBox commandBox;

    private IBattleCommands battleLogic;

    // the shared controls, so the player's rebinds actually apply here.
    // do NOT go back to `new GameControls()` -- see the comment in GameInput
    private GameControls Controls => GameInput.Controls;

    private void Awake()
    {
        battleLogic = FindFirstObjectByType<BattleController>() as IBattleCommands;

        if (battleLogic == null)
        {
            Debug.LogWarning("[BattleUIInput] >> no BattleController in the scene. the player's " +
                "choices won't go anywhere (fine for ui-only testing).");
        }
    }

    private void Update()
    {
        if (Controls.Battle.Confirm.WasPerformedThisFrame())
        {
            OnConfirmPressed();
        }

        if (Controls.Battle.Back.WasPerformedThisFrame())
        {
            OnBackPressed();
        }

        if (Controls.Battle.Pause.WasPerformedThisFrame())
        {
            TogglePause();
        }
    }


    // talking to the logic  ================================================================

    /// <summary>
    /// Sends the player's choice to the game logic.
    ///
    /// Careful with ordering around this: <c>SelectAction</c> resolves the ENTIRE turn
    /// synchronously and queues every event for it before returning. So the command box needs
    /// to be out of its selection mode before this is called, or the player could fire off a
    /// second turn while the first one's events are still draining.
    /// </summary>
    public void SubmitAction(string actionId, ActionType actionType)
    {
        if (string.IsNullOrEmpty(actionId))
        {
            Debug.LogWarning($"[BattleUIInput] >> tried to submit a {actionType} action with no id.");
            return;
        }

        if (battleLogic == null)
        {
            Debug.LogWarning($"[BattleUIInput] >> no battle logic to send '{actionId}' to.");
            return;
        }

        battleLogic.SelectAction(actionId, actionType);
    }

    /// <summary>
    /// Convenience wrapper for <c>SubmitAction</c> with a move. Same ordering warning applies.
    /// </summary>
    public void SubmitMove(string moveId)
    {
        SubmitAction(moveId, ActionType.Attacking);
    }

    /// <summary>
    /// Convenience wrapper for <c>SubmitAction</c> with a switch. Takes a shrimp's
    /// <c>instanceID</c>, not a definition id.
    /// </summary>
    public void SubmitSwitch(string shrimpInstanceId)
    {
        SubmitAction(shrimpInstanceId, ActionType.Switching);
    }

    public void TogglePause()
    {
        battleLogic?.TogglePause();
    }


    // ui-local navigation  =================================================================

    /// <summary>
    /// Flips from the move menu to the switch menu.
    ///
    /// This isn't a game action. The contract has no "player wants to switch" event --
    /// <c>SwitchingShrimp</c> is only ever a result -- so moving between the two selection
    /// menus is entirely the UI's business and the logic never hears about it.
    /// </summary>
    public void OpenShrimpSelect()
    {
        if (commandBox != null && commandBox.GetDisplayMode() == CommandBoxMode.MOVE_SELECT)
        {
            commandBox.SwitchCommandBoxDisplayMode(CommandBoxMode.SHRIMP_SELECT);
        }
    }

    /// <summary>
    /// Flips back from the switch menu to the move menu.
    /// </summary>
    public void OpenMoveSelect()
    {
        if (commandBox != null && commandBox.GetDisplayMode() == CommandBoxMode.SHRIMP_SELECT)
        {
            commandBox.SwitchCommandBoxDisplayMode(CommandBoxMode.MOVE_SELECT);
        }
    }

    private void OnConfirmPressed()
    {
        // while text is typing, confirm means "stop teasing me and show the whole line".
        // the button press itself is handled by the event system, so we only care about
        // the case where an animation is in the way
        if (uiManager != null && uiManager.IsBusy())
        {
            uiManager.ForceSkipCurrent();
        }
    }

    private void OnBackPressed()
    {
        if (commandBox == null)
            return;

        // backing out of the switch menu returns you to your moves.
        // from the move menu there's nowhere to go -- you have to pick something
        if (commandBox.GetDisplayMode() == CommandBoxMode.SHRIMP_SELECT)
        {
            OpenMoveSelect();
        }
        else if (commandBox.GetDisplayMode() == CommandBoxMode.MOVE_SELECT)
        {
            OpenShrimpSelect();
        }
    }
}
