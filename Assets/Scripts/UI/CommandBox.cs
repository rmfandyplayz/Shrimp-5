using Shrimp5.UIContract;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

// handles things related to the command box
public class CommandBox : MonoBehaviour
{
    [Header("references")]
    [SerializeField] TextMeshProUGUI promptText; // example: "your move!"
    [SerializeField] RectTransform moveButtonContainer;
    [SerializeField] GameObject moveButtonPrefab; // prefab to duplicate and change the icon & text

    private List<GameObject> spawnedButtons = new List<GameObject>(); // 4 at most ig

    public void Render(BattleSnapshot snapshot)
    {
        promptText.text = snapshot.promptText;

        // TODO: display moves

    }
}
