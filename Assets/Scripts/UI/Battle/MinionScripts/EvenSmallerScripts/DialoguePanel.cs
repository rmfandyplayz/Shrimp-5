using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// written by Claude Opus 5
// the TypeText typewriter coroutine is andy's, lifted out of the old CommandBox as is
// the text area of the command box. does two jobs:
//   1. types out battle dialogue word by word
//   2. shows the full writeup for whatever move the cursor is on during move select
//
// it stays visible (but not interactable) during move select so job 2 has somewhere to live.
public class DialoguePanel : MonoBehaviour, ICommandBoxPanel
{
    [Header("refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField, Tooltip("optional. the big icon shown next to a move's details")]
    private Image moveIcon;

    [Header("settings")]
    [SerializeField, Tooltip("how much delay between displaying each word in the dialog?")]
    private float dialogueWordDelay = 0.075f;

    CommandBoxMode ICommandBoxPanel.Mode => CommandBoxMode.DIALOGUE_DISPLAY;

    private Coroutine dialogueTextCoroutine;
    private Action pendingOnComplete;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        SetMoveIcon(null);
    }

    /// <summary>
    /// Types <paramref name="textToDisplay"/> out word by word.
    ///
    /// Interrupts whatever was already typing, so there's no need to skip first.
    /// <paramref name="onComplete"/> fires once the last word is up (or immediately if the
    /// player skips), which is how handlers know to move on to the next event.
    /// </summary>
    public void SetDialogue(string textToDisplay, Action onComplete = null)
    {
        StopTyping();
        SetMoveIcon(null);

        if (dialogueText == null)
        {
            Debug.LogWarning($"[DialoguePanel] >> {name} has no dialogueText assigned.");
            onComplete?.Invoke();
            return;
        }

        // can't run a coroutine on an inactive object. shouldn't happen since CommandBox
        // switches to dialogue mode before calling this, but don't soft-lock the event queue
        // waiting on a callback that can never fire
        if (!gameObject.activeInHierarchy)
        {
            dialogueText.text = textToDisplay;
            dialogueText.maxVisibleCharacters = int.MaxValue;
            onComplete?.Invoke();
            return;
        }

        pendingOnComplete = onComplete;
        dialogueTextCoroutine = StartCoroutine(TypeText(textToDisplay));
    }

    /// <summary>
    /// Dumps the full line instantly and fires the pending completion callback.
    /// Safe to call when nothing is typing.
    /// </summary>
    public void SkipDialogue()
    {
        if (dialogueTextCoroutine == null)
            return;

        StopTyping();

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        }

        FireOnComplete();
    }

    /// <summary>
    /// Whether a line is still being typed out. Useful for deciding whether a confirm press
    /// means "skip the text" or "act on what's on screen".
    /// </summary>
    public bool IsTyping()
    {
        return dialogueTextCoroutine != null;
    }

    /// <summary>
    /// Swaps the text area over to a move's full details (damage/healing, effect applied,
    /// icon, lore blurb). Shown while the player is browsing the move buttons.
    /// </summary>
    public void ShowMoveDetails(MoveDefinition move)
    {
        StopTyping();
        pendingOnComplete = null;

        if (dialogueText != null)
        {
            dialogueText.text = BattleTextBuilder.MoveDetails(move);
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        // TODO: revisit alongside MoveCategory once owen settles how categories are stored
        SetMoveIcon(move != null ? MoveCategories.GetIcon(move) : null);
    }

    /// <summary>
    /// Puts a line up instantly with no typewriter. Handy for prompts that shouldn't
    /// make the player wait, like "what should X do?".
    /// </summary>
    public void SetDialogueInstant(string textToDisplay)
    {
        StopTyping();
        pendingOnComplete = null;
        SetMoveIcon(null);

        if (dialogueText != null)
        {
            dialogueText.text = textToDisplay;
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }
    }

    void ICommandBoxPanel.Show(bool interactable)
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
            canvasGroup.alpha = 1f;
        }
    }

    void ICommandBoxPanel.Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    // cleared whenever we go back to plain dialogue, so a move's icon doesn't linger
    private void SetMoveIcon(Sprite sprite)
    {
        if (moveIcon == null)
            return;

        moveIcon.sprite = sprite;
        moveIcon.enabled = sprite != null;
    }

    // safe to call when nothing's typing. the old version of this NREd because it called
    // StopCoroutine on a null handle once the line had finished on its own
    private void StopTyping()
    {
        if (dialogueTextCoroutine == null)
            return;

        StopCoroutine(dialogueTextCoroutine);
        dialogueTextCoroutine = null;
    }

    // hands control back to whoever was waiting on this line
    private void FireOnComplete()
    {
        // null it out before invoking, in case the callback kicks off another line
        Action callback = pendingOnComplete;
        pendingOnComplete = null;
        callback?.Invoke();
    }

    // does the typewriter effect on the dialog box (word by word instead as requested by casdneara)
    IEnumerator TypeText(string textToDisplay)
    {
        dialogueText.text = textToDisplay;
        dialogueText.maxVisibleCharacters = 0;

        string[] words = textToDisplay.Split(' ');
        int currentCharCount = 0;

        for (int i = 0; i < words.Length; i++)
        {
            currentCharCount += words[i].Length;

            // add 1 for the space that's just skipped
            if (i < words.Length - 1)
            {
                currentCharCount++;
            }

            dialogueText.maxVisibleCharacters = currentCharCount;

            yield return new WaitForSeconds(dialogueWordDelay);
        }

        // not necessary, but safety net to ensure everything's visible
        dialogueText.maxVisibleCharacters = textToDisplay.Length;
        dialogueTextCoroutine = null;

        FireOnComplete();
    }
}
