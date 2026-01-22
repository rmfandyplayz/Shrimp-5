using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFeedback_ExitConfirm : MonoBehaviour, IMenuFeedback
{
    [SerializeField] private float goBackTime; // how long to wait for user to confirm that they want to quit the game
    [SerializeField] private TextMeshProUGUI confirmText;
    
    private bool canQuit = false;
    
    public void OnSelect(){}

    public void OnDeselect(){}

    public void OnSubmit()
    {
        if (canQuit)
        {
            MenuManager.Instance.QuitGame();
        }
        else
        {
            StartCoroutine(ExitConfirmCountdown());
        }
    }

    IEnumerator ExitConfirmCountdown()
    {
        confirmText.text = "are u sure?";
        canQuit = true;
        
        yield return new WaitForSeconds(goBackTime);
        confirmText.text = "Exit";
        canQuit = false;
    }
}
