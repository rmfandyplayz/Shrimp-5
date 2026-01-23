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

    private bool isEnabled = false;
    private bool canQuit = false; // is the quit button "active" as in is it currently on the "r u sure about that?" text
    private bool isQuitting = false; // if the game isn't in a quit coroutine

    private void Awake()
    {
        translucentBackground.color = new Color(0, 0, 0, 0);

    }

    public override void OnBackPressed()
    {
        if(!isQuitting)
            ResumeGame();
    }

    public void ResumeGame()
    {
        battleController.PauseToggle();

        AnimateOut(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            // disable gameobject if animate out doesn't handle it well enough
        });
    }

    public void OpenSettings()
    {
        // TODO: potentially set game time to zero????
        AnimateIn(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        });
    }

    public void QuitToMainMenu()
    {
        if(canQuit && !isQuitting)
        {
            isQuitting = true;
            StartCoroutine(QuitToMainMenuSequence());
        }
        else if (!isQuitting)
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

        sequence.Insert(0, cg.transform.DOMoveY(-720, 0.5f).SetEase(Ease.OutBack));
        sequence.Insert(0, translucentBackground.DOFade(1, 0.35f));

        sequence.SetUpdate(true);

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public override void AnimateOut(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0, cg.transform.DOMoveY(-720, 0.5f).From().SetEase(Ease.OutBack));
        sequence.Insert(0, translucentBackground.DOFade(1, 0.35f).From());

        sequence.SetUpdate(true);

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public bool GetEnabled()
    {
        return isEnabled;
    }
}
