using Shrimp5.UIContract;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

// written by andy
// handles things related to the command box
public class CommandBox : MonoBehaviour
{
    [Header("references")]
    [SerializeField] TextMeshProUGUI promptText; // example: "your move!"
    [SerializeField] RectTransform moveButtonContainer;
    [SerializeField] MoveButton moveButtonPrefab; // prefab to duplicate and change the icon & text

    private List<MoveButton> spawnedButtons = new List<MoveButton>(); // 4 at most ig

    public void Render(BattleSnapshot snapshot)
    {
        promptText.text = snapshot.promptText;

        List<MoveData> moves = snapshot.moves;

        //ensure enough buttons are there
        while(spawnedButtons.Count < moves.Count)
        {
            MoveButton newButton = Instantiate(moveButtonPrefab, moveButtonContainer);
            spawnedButtons.Add(newButton);
        }

        for(int i = 0; i < spawnedButtons.Count; i++ )
        {
            if(i < moves.Count)
            {
                spawnedButtons[i].gameObject.SetActive(true);
                bool isSelected = i == snapshot.selectedIndex;
                spawnedButtons[i].Configure(moves[i], isSelected);
            }
            else // hide extra buttons
            {
                spawnedButtons[i].gameObject.SetActive(false);
            }
        }

        SetSelection(snapshot.selectedIndex);
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
}
