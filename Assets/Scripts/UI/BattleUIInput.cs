using UnityEngine;
using Shrimp5.UIContract;

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
    private void OnEnable() => gameControls.Battle.Enable();
    private void OnDisable() => gameControls.Battle.Disable();

    private void Update()
    {
        BattleSnapshot snapshot = battleModel.GetSnapshot();

        // TODO: potentially different things we can do for pause n stuff
        if(snapshot.battleMode != BattleUIMode.ChoosingAction && 
            snapshot.battleMode != BattleUIMode.ChoosingSwitchTeammate)
        {
            return;
        }

        // navigation
        if (gameControls.Battle.NavRight.WasPerformedThisFrame())
        {
            MoveSelectionCursor(1, snapshot.moves.Count);
        }
        else if (gameControls.Battle.NavLeft.WasPerformedThisFrame())
        {
            MoveSelectionCursor(-1, snapshot.moves.Count);
        }

        // confirm
        if (gameControls.Battle.Confirm.WasPerformedThisFrame())
        {
            battleController.Confirm(cursorIndex);
        }

        // back
        if (gameControls.Battle.Confirm.WasPerformedThisFrame())
        {
            battleController.Back();
        }

        // inspect
        if (gameControls.Battle.Inspect_Secondary.WasPerformedThisFrame())
        {
            battleController.Inspect(cursorIndex);
        }
    }

    private void MoveSelectionCursor(int direction, int totalMoves)
    {
        cursorIndex += direction;
        if (cursorIndex >= totalMoves)
            cursorIndex = 0;
        if (cursorIndex < 0)
            cursorIndex = totalMoves - 1;

        commandBox.SetSelection(cursorIndex);
    }

}
