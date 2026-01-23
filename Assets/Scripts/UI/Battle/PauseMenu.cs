using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// written by andy
// handles the battle pause menu. this code is shit but i'm running low on time so i can't engineer
// yet another framework :salute:
public class PauseMenu : MenuBase
{
    [SerializeField] BattleController battleController;
    [SerializeField] Image translucentBackground;
    [SerializeField] TextMeshProUGUI returnMenuText;
    [Header("sprite refs")]
    [SerializeField] Image resumeIcon;
    [SerializeField] Image pauseIcon;

    private static bool isEnabled = false;
    private static bool isAnimating = false; // if if some form of animation is in progress and shouldn't be disturbed
    private bool canQuit = false; // is the quit button "active" as in is it currently on the "r u sure about that?" text

    private Vector2 menuOriginalPos;

    public override void Awake()
    {
        base.Awake();
        
        menuOriginalPos = cg.transform.position;
        translucentBackground.color = new Color(0, 0, 0, 0);
    }

    public override void OnBackPressed()
    {
        //if(!isAnimating)
        //    ResumeGame();
    }

    public void ResumeGame()
    {
        battleController.PauseToggle(); // tell logic to unpause
        CloseVisuals();
    }

    public void CloseVisuals()
    {
        isAnimating = true;
        cg.interactable = false;

        AnimateOut(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            isEnabled = false;
            isAnimating = false;
        });
    }

    public void OpenSettings()
    {
        // TODO: potentially set game time to zero????

        isEnabled = true;
        isAnimating = true;
        AnimateIn(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
            isAnimating = false;
            cg.interactable = true;
        });
    }


    public void QuitToMainMenu()
    {
        if(canQuit && !isAnimating)
        {
            isAnimating = true;
            StartCoroutine(QuitToMainMenuSequence());
        }
        else if (!isAnimating && !canQuit)
        {
            StartCoroutine(QuitToMainMenuButtonSequence());
        }
    }

    IEnumerator QuitToMainMenuButtonSequence()
    {
        returnMenuText.text = "are u sure about that?";
        canQuit = true;
        yield return new WaitForSeconds(3);

        returnMenuText.text = "Return to Menu";
        canQuit = false;
    }

    IEnumerator QuitToMainMenuSequence()
    {
        // call scene transition
        yield return null;
    }

    public override void AnimateIn(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0, cg.transform.DOMoveY(400, 0.5f).SetEase(Ease.OutExpo));
        sequence.Insert(0, translucentBackground.DOColor(new Color(0, 0, 0, 0.6f), 0.35f));

        sequence.SetUpdate(true);

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public override void AnimateOut(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0, cg.transform.DOMove(menuOriginalPos, 0.5f).SetEase(Ease.OutExpo));
        sequence.Insert(0, translucentBackground.DOFade(0, 0.35f));

        sequence.SetUpdate(true);

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public static bool GetEnabled()
    {
        return isEnabled;
    }

    public static bool GetAnimating()
    {
        return isAnimating;
    }
}
