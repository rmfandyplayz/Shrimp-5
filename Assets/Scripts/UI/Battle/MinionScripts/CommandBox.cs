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
    [SerializeField] CanvasGroup dialogueCanvasGroup;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField, Tooltip("how much delay between displaying each word in the dialog?")]
    float dialogueWordDelay;

    [Header("move selection mode refs")]
    [SerializeField] CanvasGroup moveSelectionGroup;
    private List<GameObject> moveSelectionTiles;

    [Header("shrimp selection mode refs")]
    [SerializeField] CanvasGroup shrimpSelectionGroup;
    private List<GameObject> shrimpSelectionTiles;

    [Header("misc")]
    [SerializeField] TextMeshProUGUI promptText;
    [SerializeField] GameObject selectionTilePrefab; // will be duplicated

    private CommandBoxMode currentDisplayMode; // default display type
    private Coroutine dialogueTextCoroutine;


    private void Awake()
    {
        moveSelectionTiles = new();
        shrimpSelectionTiles = new();
        SwitchCommandBoxDisplayMode(CommandBoxMode.DIALOGUE_DISPLAY);
    }
    
    public void SwitchCommandBoxDisplayMode(CommandBoxMode newDisplayMode)
    {
        if (newDisplayMode == currentDisplayMode)
            return;

        // TODO: maybe add animation?????
        if(currentDisplayMode == CommandBoxMode.DIALOGUE_DISPLAY)
        {
            moveSelectionGroup.interactable = false;
            moveSelectionGroup.gameObject.SetActive(false);
            shrimpSelectionGroup.interactable = false;
            shrimpSelectionGroup.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);

            dialogueCanvasGroup.interactable = true;
            dialogueCanvasGroup.gameObject.SetActive(true);
        }
        else if(currentDisplayMode == CommandBoxMode.MOVE_SELECT)
        {
            dialogueCanvasGroup.interactable = false;
            dialogueCanvasGroup.gameObject.SetActive(false);
            shrimpSelectionGroup.interactable = false;
            shrimpSelectionGroup.gameObject.SetActive(false);

            moveSelectionGroup.interactable = true;
            moveSelectionGroup.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(moveSelectionTiles[0]);
        }
        else if(currentDisplayMode == CommandBoxMode.SHRIMP_SELECT)
        {
            moveSelectionGroup.interactable = false;
            moveSelectionGroup.gameObject.SetActive(false);
            dialogueCanvasGroup.interactable = false;
            dialogueCanvasGroup.gameObject.SetActive(false);

            shrimpSelectionGroup.interactable = true;
            shrimpSelectionGroup.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(shrimpSelectionTiles[0]);
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