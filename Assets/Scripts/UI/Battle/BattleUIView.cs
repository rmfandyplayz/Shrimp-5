using Shrimp5.UIContract;
using TMPro;
using UnityEngine;
using DG.Tweening;

// written by andy
// this is THE ONLY SCRIPT that talks to BattleUIModel.
public class BattleUIView : MonoBehaviour
{
    [Header("data source")]
    [SerializeField] BattleUIModel battleModel;

    [Header("minion scripts")]
    [SerializeField] UnitFrame playerFrame;
    [SerializeField] UnitFrame enemyFrame;
    [SerializeField] CommandBox commandBox;
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] RectTransform tooltipPanel;
    [SerializeField] TextMeshProUGUI tooltipText;

    void Start()
    {
        if(battleModel != null)
        {
            battleModel.Changed += OnModelChanged;

            OnModelChanged();
        }
    }

    private void OnDestroy()
    {
        if(battleModel != null) // unsubscribe
        {
            battleModel.Changed -= OnModelChanged;
        }
    }

    private void OnModelChanged()
    {
        BattleSnapshot snapshot = battleModel.GetSnapshot();

        if (snapshot.battleMode == BattleUIMode.Paused)
        {
            // already paused
            if (!PauseMenu.GetEnabled())
            {
                pauseMenu.OpenSettings();
            }

            return;
        }
        else
        {
            // not paused; ensure menu is gone
            if (PauseMenu.GetEnabled())
            {
                pauseMenu.CloseVisuals();
            }
        }

        // minion scripts
        playerFrame.Render(snapshot.playerInfoData);
        enemyFrame.Render(snapshot.enemyInfoData);

        commandBox.Render(snapshot);

        // tooltips
        if (snapshot.tooltipData.isVisible)
        {
            tooltipText.text = snapshot.tooltipData.text;
            tooltipPanel.gameObject.SetActive(true);
        }
        else
        {
            tooltipPanel.gameObject.SetActive(false);
            tooltipText.text = string.Empty;
        }
    }
}
