using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shrimp5.UIContract;

// written by andy
// utility scripts for each MoveButton
public class MoveButton : MonoBehaviour
{
    [Header("references")]
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI moveNameText;
    [SerializeField] TextMeshProUGUI moveDescriptionText;
    [SerializeField] GameObject selectionHighlight;

    public void Configure(ButtonData moveData, bool isSelected)
    {
        moveNameText.text = moveData.moveName;
        moveDescriptionText.text = moveData.moveShortDescription;

        iconImage.sprite = SpriteResolver.Get(moveData.iconID);

        selectionHighlight.SetActive(isSelected);

        if (moveData.isEnabled)
        {
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.color = Color.gray;
        }
    }


    public GameObject GetSelectionHighlight()
    {
        return selectionHighlight; 
    }
}
