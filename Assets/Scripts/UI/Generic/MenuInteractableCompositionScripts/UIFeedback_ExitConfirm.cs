using System.Collections;
using TMPro;
using UnityEngine;

// written by andy
// hardened by Claude Opus 5
// makes the exit button ask "are u sure?" before it actually quits
public class UIFeedback_ExitConfirm : MonoBehaviour, IMenuFeedback
{
    [SerializeField] private float goBackTime; // how long to wait for user to confirm that they want to quit the game
    [SerializeField] private TextMeshProUGUI confirmText;

    private bool canQuit = false;
    private Coroutine countdownRoutine;

    // whatever the button said to begin with. was hardcoded back to "Exit" before, which
    // silently renamed the button if it ever said anything else
    private string originalLabel;

    private void Awake()
    {
        if (confirmText != null)
        {
            originalLabel = confirmText.text;
        }
    }

    public void OnSelect(){}

    public void OnDeselect()
    {
        // wandering off the button shouldn't leave it armed and still reading "are u sure?"
        ResetConfirmState();
    }

    public void OnSubmit()
    {
        if (canQuit)
        {
            MenuManager.Instance.QuitGame();
            return;
        }

        // restart the window rather than stacking a second countdown that would disarm the
        // button early
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
        }

        countdownRoutine = StartCoroutine(ExitConfirmCountdown());
    }

    // arms the button for goBackTime seconds, then puts it back to normal
    IEnumerator ExitConfirmCountdown()
    {
        confirmText.text = "are u sure?";
        canQuit = true;

        yield return new WaitForSeconds(goBackTime);

        countdownRoutine = null;
        ResetConfirmState();
    }

    private void ResetConfirmState()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        if (!canQuit)
            return;

        canQuit = false;

        if (confirmText != null)
        {
            confirmText.text = originalLabel;
        }
    }
}
