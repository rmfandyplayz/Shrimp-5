using Shrimp5.UIContract;
using TMPro;
using UnityEngine;

// written by andy
// this is THE ONLY SCRIPT that talks to BattleUIModel.
public class BattleUIView : MonoBehaviour
{
    [Header("data source")]
    [SerializeField] BattleUIModel battleModel;

    [Header("sub-views (minion scripts)")]
    [SerializeField] UnitFrame playerFrame;
    [SerializeField] UnitFrame enemyFrame;
    [SerializeField] CommandBox commandBox;
    [SerializeField] RectTransform pauseMenu;
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

        if(snapshot.battleMode == BattleUIMode.Paused)
        {
            pauseMenu.gameObject.SetActive(true);
            return;
        }

        pauseMenu.gameObject.SetActive(false);

        // minion scripts
        playerFrame.Render(snapshot.playerInfoData);
        enemyFrame.Render(snapshot.enemyInfoData);

        commandBox.Render(snapshot);

        // tooltips
        if (snapshot.tooltipData.isVisible)
        {
            tooltipPanel.gameObject.SetActive(true);
            tooltipText.text = snapshot.tooltipData.body;
        }
        else
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }
}
