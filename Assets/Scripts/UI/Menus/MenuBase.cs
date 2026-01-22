using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;

// written by andy
// the base class for all menu animations
public class MenuBase : MonoBehaviour
{

    [SerializeField]
    [Tooltip("leave blank if it doesn't apply!")]
    protected MenuBase backMenu; // menu will go back to this
    [SerializeField]
    [Tooltip("the first selected element when opening this menu")]
    protected GameObject firstSelected; // the first selected element when opening this menu

    protected CanvasGroup cg;
    protected GameControls gameControls;
    protected Vector2 defaultPos;
    protected Vector3 defaultScale;

    public virtual void Awake()
    {
        if(cg == null)
        {
            cg = GetComponentInChildren<CanvasGroup>();
        }
        gameControls = new();

        // store default values so it can be reset
        if(transform is RectTransform rectTransform)
            defaultPos = rectTransform.anchoredPosition;

        defaultScale = transform.localScale;
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

    // completely resets the state of things in the menu for them to be tweened again
    // probably should override this in inheritor classes for custom implementation.
    public virtual void ResetState(bool resetAlpha = false)
    {
        transform.DOKill();
        cg.DOKill();

        if (transform is RectTransform rectTransform)
            rectTransform.anchoredPosition = defaultPos;

        transform.localScale = defaultScale;

        if(resetAlpha)
            cg.alpha = 0;
    }


    // optional implementable methods vvvvvvvvvvvvvvvvvv


    public virtual void AnimateIn(Action onComplete)
    {
        ResetState();

        cg.alpha = 0;
        cg.gameObject.SetActive(true);
        cg.DOFade(1, 0.2f).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
    }

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
}
