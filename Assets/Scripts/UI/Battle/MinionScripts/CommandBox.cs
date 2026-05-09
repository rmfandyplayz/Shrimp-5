using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using Sh.UIContract;

// handles all VISUAL aspects of the box at the bottom. whether that be choosing moves,
// going through dialogue, etc.
// written by andy
public class CommandBox : MonoBehaviour
{
    [Header("dialogue mode refs")]
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField, Tooltip("how much delay between displaying each word in the dialog?")]
    private float dialogueWordDelay;

    [Header("move selection mode refs")]
    [SerializeField] private CanvasGroup moveSelectionGroup;
    [SerializeField] private List<GameObject> moveSelectionTiles;

    [Header("shrimp selection mode refs")]
    [SerializeField] CanvasGroup shrimpSelectionGroup;
    [SerializeField] private List<GameObject> shrimpSelectionTiles;

    [Header("misc")]
    [SerializeField] private GameObject selectionTilePrefab; // will be duplicated

    private CommandBoxMode currentDisplayMode; // default display type
    private Coroutine dialogueTextCoroutine;


    // TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER 
    public void SwitchToShrimpSelect()
    {
        SwitchCommandBoxDisplayMode(CommandBoxMode.SHRIMP_SELECT);
    }

    public void SwitchToMoveSelect()
    {
        SwitchCommandBoxDisplayMode(CommandBoxMode.MOVE_SELECT);
    }
    public void SwitchToDialogue()
    {
        SwitchCommandBoxDisplayMode(CommandBoxMode.DIALOGUE_DISPLAY);
    }
    // TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER 


    public void SwitchCommandBoxDisplayMode(CommandBoxMode newDisplayMode)
    {
        if (newDisplayMode == currentDisplayMode)
            return;

        if(newDisplayMode == CommandBoxMode.DIALOGUE_DISPLAY) // display text
        {
            moveSelectionGroup.interactable = false;
            moveSelectionGroup.gameObject.SetActive(false);
            shrimpSelectionGroup.interactable = false;
            shrimpSelectionGroup.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);

            dialogueCanvasGroup.interactable = true;
            dialogueCanvasGroup.gameObject.SetActive(true);

            currentDisplayMode = newDisplayMode;
        }
        else if(newDisplayMode == CommandBoxMode.MOVE_SELECT) // shrimp select move
        {
            dialogueCanvasGroup.interactable = false;
            dialogueCanvasGroup.gameObject.SetActive(false);
            shrimpSelectionGroup.interactable = false;
            shrimpSelectionGroup.gameObject.SetActive(false);

            moveSelectionGroup.interactable = true;
            moveSelectionGroup.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(moveSelectionTiles[0]);

            currentDisplayMode = newDisplayMode;
        }
        else if(newDisplayMode == CommandBoxMode.SHRIMP_SELECT) // select another team member
        {
            moveSelectionGroup.interactable = false;
            moveSelectionGroup.gameObject.SetActive(false);
            dialogueCanvasGroup.interactable = false;
            dialogueCanvasGroup.gameObject.SetActive(false);

            shrimpSelectionGroup.interactable = true;
            shrimpSelectionGroup.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(shrimpSelectionTiles[0]);

            currentDisplayMode = newDisplayMode;
        }
    }

    public void StartDialogueTyping(string textToDisplay) // note: it's recommend to use skipdialog() before running this:
    {
        dialogueTextCoroutine = StartCoroutine(TypeText(textToDisplay));
    }

    public void SkipDialog() // stops the typing coroutine and displays full text
    {
        StopCoroutine(dialogueTextCoroutine);
        dialogueTextCoroutine = null;
        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
    }

    // does the typewriter effect on the dialog box (word by word instead as requested by casdneara)
    IEnumerator TypeText(string textToDisplay)
    {
        dialogueText.text = textToDisplay;
        dialogueText.maxVisibleCharacters = 0;

        string[] words = textToDisplay.Split(' ');
        int currentCharCount = 0;

        for(int i = 0; i < words.Length; i++)
        {
            currentCharCount += words[i].Length;

            // add 1 for the space that's just skipped 
            if(i < words.Length - 1)
            {
                currentCharCount++;
            }

            dialogueText.maxVisibleCharacters = currentCharCount;

            yield return new WaitForSeconds(dialogueWordDelay);
        }

        // not necessary, but safety net to ensure everything's visible
        dialogueText.maxVisibleCharacters = textToDisplay.Length;
        dialogueTextCoroutine = null;
    }
}

public enum CommandBoxMode
{
    DIALOGUE_DISPLAY,
    MOVE_SELECT,
    SHRIMP_SELECT
}