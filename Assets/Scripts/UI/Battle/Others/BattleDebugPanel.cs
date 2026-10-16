using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// written by Claude Opus 5.5
// the on-screen button grid for BattleUIDebugDriver, so the battle test scene can be driven
// from the keyboard instead of right-clicking a component header.
//
// the buttons are plain unity Buttons wired straight to the driver in the inspector, and they
// navigate like any other button. this script only does two things:
//   1. keeps the "who are events pointed at" readout current
//   2. puts the cursor back on a debug button whenever the selection falls off the screen.
//      the command box switches its tiles off while dialogue plays, and a switched-off
//      selection can't receive arrow keys -- without this you'd be left with no cursor at all
//      after the first turn
public class BattleDebugPanel : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private BattleUIDebugDriver driver;
    [SerializeField, Tooltip("shows who the event buttons are currently pointed at")]
    private TextMeshProUGUI targetLabel;
    [SerializeField, Tooltip("where the cursor goes when nothing else on screen is selected")]
    private Selectable fallbackSelection;

    // so the label only rebuilds when the target actually changes
    private string shownTarget;

    private void Update()
    {
        RefreshTargetLabel();
        KeepSomethingSelected();
    }

    private void RefreshTargetLabel()
    {
        if (targetLabel == null || driver == null)
            return;

        string target = driver.GetTargetDescription();

        if (target == shownTarget)
            return;

        shownTarget = target;
        targetLabel.text = "events target: " + target;
    }

    private void KeepSomethingSelected()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null || fallbackSelection == null)
            return;

        GameObject selected = eventSystem.currentSelectedGameObject;

        if (selected != null && selected.activeInHierarchy)
            return;

        if (!fallbackSelection.gameObject.activeInHierarchy || !fallbackSelection.IsInteractable())
            return;

        eventSystem.SetSelectedGameObject(fallbackSelection.gameObject);
    }
}
