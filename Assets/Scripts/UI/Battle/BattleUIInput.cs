using Shrimp5.UIContract;
using UnityEngine;
using UnityEngine.InputSystem.XR;

// written by andy
// communicates with game logic on what input has been taken
public class BattleUIInput : MonoBehaviour
{
    [Header("references")]
    [SerializeField] BattleUIModel battleModel;
    [SerializeField] BattleController battleController;
    [SerializeField] CommandBox commandBox;

    private int cursorIndex; // keeps track of where selection cursor is

    private GameControls gameControls;

    private void Awake()
    {
        gameControls = new GameControls();
    }

    // apparently i gotta do this
    private void OnEnable()
    {
        gameControls.Battle.Enable();
    }
    private void OnDisable()
    {
        gameControls.Battle.Disable();
    }
    

    private void Update()
    {
        BattleSnapshot snapshot = battleModel.GetSnapshot();

        // resolving action context-based actions
        if (snapshot.battleMode == BattleUIMode.ResolvingAction)
        {
            // z key
            if (gameControls.Battle.Confirm.WasPerformedThisFrame())
            {
                if (!commandBox.IsTyping())
                {
                    battleController.DialogueConfirm();
                }
            }

            // x key
            if (gameControls.Battle.Back.WasPerformedThisFrame() && !PauseMenu.GetEnabled())
            {
                commandBox.SkipTyping();
            }

            // c key
            if (gameControls.Battle.Inspect_Secondary.WasPerformedThisFrame())
            {
                battleController.DialogueSkipAll();
            }
        }

        // action choosing context-bsed actions
        if (snapshot.battleMode == BattleUIMode.ChoosingAction ||
            snapshot.battleMode == BattleUIMode.ChoosingSwitchTeammate)
        {
            // navigation
            if (gameControls.Battle.NavRight.WasPerformedThisFrame())
            {
                MoveSelectionCursor(1, snapshot.buttons.Count);
            }
            else if (gameControls.Battle.NavLeft.WasPerformedThisFrame())
            {
                MoveSelectionCursor(-1, snapshot.buttons.Count);
            }

            // confirm
            if (gameControls.Battle.Confirm.WasPerformedThisFrame())
            {
                battleController.Confirm(cursorIndex);
            }

            // back
            if (gameControls.Battle.Confirm.WasPerformedThisFrame() && !PauseMenu.GetEnabled())
            {
                battleController.Back();
            }

            // inspect
            if (gameControls.Battle.Inspect_Secondary.WasPerformedThisFrame())
            {
                battleController.Secondary(cursorIndex);
            }
        }
        
        // non context-dependent actions
        if (gameControls.Battle.Pause.WasPerformedThisFrame() && !PauseMenu.GetAnimating())
        {
            battleController.PauseToggle();
        }
    }

    private void MoveSelectionCursor(int direction, int totalMoves)
    {
        if (PauseMenu.GetEnabled())
            return;

        cursorIndex += direction;
        if (cursorIndex >= totalMoves)
            cursorIndex = 0;
        if (cursorIndex < 0)
            cursorIndex = totalMoves - 1;

        commandBox.SetSelection(cursorIndex);
    }

    public int GetSelectionIndex()
    {
        return cursorIndex;
    }

}
