using Shrimp5.UIContract;
using TMPro;
using UnityEngine;

// handles things related to the command box
public class CommandBox : MonoBehaviour
{
    [Header("references")]
    [SerializeField] TextMeshProUGUI promptText; // example: "your move!"

    public void Render(BattleSnapshot snapshot)
    {

    }
}
