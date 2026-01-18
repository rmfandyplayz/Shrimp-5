using Shrimp5.UIContract;
using UnityEngine;

public class BattleController : MonoBehaviour, IBattleUIActions
{
    [Header("Refs")]
    [SerializeField] private BattleUIModel uiModel;
    private BattleSnapshot currentSnapshot;
    void Start()
    {
        currentSnapshot = new BattleSnapshot();
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        UpdateUI();
    }
    private void UpdateUI()
    {
        uiModel.SetSnapshot(currentSnapshot);
    }
    public void Back()
    {
        if (currentSnapshot.battleMode == BattleUIMode.InspectingMove)
        {
            currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        }
        UpdateUI();
    }

    public void Confirm(int index)
    {
        if (currentSnapshot.battleMode == BattleUIMode.ChoosingAction)
        {
            if (index == 3)
            {
                currentSnapshot.battleMode = BattleUIMode.ChoosingSwitchTeammate;
            }
            else
            {
            currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
            }
        }
        else if (currentSnapshot.battleMode == BattleUIMode.ChoosingSwitchTeammate)
        {
            if (index == 3)
            {
                currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
            }
            else
            {
            currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
            }
        }
        UpdateUI();
    }

    public void Inspect(int index)
    {
        currentSnapshot.battleMode = BattleUIMode.InspectingMove;
        UpdateUI();
    }

    public void PauseToggle()
    {
        if (currentSnapshot.battleMode == BattleUIMode.Paused)
        {
            currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        } 
        else
        {
            currentSnapshot.battleMode = BattleUIMode.Paused;
        }
        UpdateUI();
    }

    public void SetTooltipTarget(string tooltipID)
    {
        UpdateUI();
    }
}
