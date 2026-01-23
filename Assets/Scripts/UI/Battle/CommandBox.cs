using Shrimp5.UIContract;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

// written by andy
// handles things related to the command box (dialogue, move selection)
public class CommandBox : MonoBehaviour
{
    [Header("references")]
    [SerializeField] TextMeshProUGUI promptText; // example: "your move!"
    [SerializeField] TextMeshProUGUI flavorText;
    [SerializeField] RectTransform moveButtonContainer;
    [SerializeField] MoveButton moveButtonPrefab; // prefab to duplicate and change the icon & text
    [SerializeField, Tooltip("time between each letter being displayed. lower means faster.")] float typewriterEffectSpeed;

    private List<MoveButton> spawnedButtons; // 4 at most ig

    private bool isTyping = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;
    private BattleUIInput battleInput;
    private BattleSnapshot currentSnapshot;

    private void Awake()
    {
        spawnedButtons = new();
        battleInput = FindFirstObjectByType<BattleUIInput>();
    }

    public void Render(BattleSnapshot snapshot)
    {
        currentSnapshot = snapshot;
        promptText.text = snapshot.promptText;
        List<ButtonData> moves = snapshot.buttons;
        bool isCutscene = snapshot.battleMode == BattleUIMode.ResolvingAction; // whether to show flavor text, or whatever of that sort

        if (isCutscene) // show text
        {
            promptText.text = snapshot.promptText;
            SetFlavorText(snapshot.flavorText);
            moveButtonContainer.gameObject.SetActive(false);
        }
        else // show buttons
        {
            promptText.text = snapshot.promptText;
            flavorText.gameObject.SetActive(false);
            moveButtonContainer.gameObject.SetActive(true);

            //render buttons vvv


            while (spawnedButtons.Count < moves.Count)
            {
                MoveButton newButton = Instantiate(moveButtonPrefab, moveButtonContainer);
                spawnedButtons.Add(newButton);
            }

            for(int i = 0; i < spawnedButtons.Count; i++)
            {
                if(i < moves.Count)
                {
                    spawnedButtons[i].gameObject.SetActive(true);
                    bool isSelected = i == battleInput.GetSelectionIndex();
                    spawnedButtons[i].Configure(moves[i], isSelected);
                }
                else // hide extra buttons
                {
                    spawnedButtons[i].gameObject.SetActive(false);
                }
            }

            SetSelection(battleInput.GetSelectionIndex());
        }
    }

    public void SetSelection(int index)
    {
        for(int i = 0; i < spawnedButtons.Count; i++)
        {
            if (!spawnedButtons[i].gameObject.activeSelf)
                continue;

            bool isSelected = i == index;
            spawnedButtons[i].GetSelectionHighlight().SetActive(isSelected);
        }
    }
    
    public void SetFlavorText(string text)
    {
        // dont restart if same text
        if (currentFullText == text)
            return;

        currentFullText = text;
        flavorText.text = text;
        flavorText.maxVisibleCharacters = 0;
        flavorText.gameObject.SetActive(true);

        if(typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypewriterCoroutine());
    }

    private IEnumerator TypewriterCoroutine()
    {
        isTyping = true;

        flavorText.ForceMeshUpdate();
        int totalChars = flavorText.textInfo.characterCount;
        flavorText.maxVisibleCharacters = 0;

        // reveal a character every typewriterEffectSpeed seconds
        for(int i = 0; i <= totalChars; i++)
        {
            flavorText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typewriterEffectSpeed);
        }

        isTyping = false;
    }

    public void SkipTyping()
    {
        if(isTyping)
        {
            if(typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            flavorText.maxVisibleCharacters = int.MaxValue;
            isTyping= false;
        }
    }
    
    public bool IsTyping()
    {
        return isTyping; 
    }
}
