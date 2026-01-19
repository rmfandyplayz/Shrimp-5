using UnityEngine;
using DG.Tweening;
using System;

// written by andy
// the base class for all menu animations
[RequireComponent(typeof(CanvasGroup))]
public class MenuBase : MonoBehaviour
{
    CanvasGroup cg;
    GameControls gameControls;

    [SerializeField] MenuBase backMenu; // menu will go back to this
    [SerializeField] GameObject firstSelected; // the first selected element when opening this menu


    public virtual void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        gameControls = new();
    }

    public virtual void Update()
    {
        if (cg.interactable)
        {
            if (gameControls.Battle.Back.WasPerformedThisFrame())
            {
                OnBackPressed();
            }
        }
    }

    // manager calls this. override this to do more funny animations rather than just fade in/out
    public virtual void AnimateIn(Action onComplete)
    {
        cg.alpha = 0;
        cg.gameObject.SetActive(true);
        cg.DOFade(1, 0.2f).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
    }

    // manager calls this. override to do custom animation
    public virtual void AnimateOut(Action onComplete)
    {
        cg.DOFade(0, 0.2f).SetUpdate(true).OnComplete(() =>
        {
            cg.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public virtual void OnBackPressed()
    {
        if(backMenu != null)
        {
            MenuManager.Instance.SwitchMenu(backMenu);
        }
    }

    private void OnEnable()
    {
        gameControls.Battle.Enable();
    }

    private void OnDisable()
    {
        gameControls.Battle.Disable();
    }

    // getters vvvvvvv (wow i'm being such a goody two shoes by following what CS1420 taught me)

    public CanvasGroup GetCanvasGroup()
    {
        return cg; 
    }

    public GameObject GetFirstSelected()
    {
        return firstSelected; 
    }
}
