using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;

// written by andy
// the base class for all menu animations
[RequireComponent(typeof(CanvasGroup))]
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
        cg = GetComponent<CanvasGroup>();
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


    // optional implementable methods vvvvvvvvvvvvvvvvvv


    public virtual void AnimateIn(Action onComplete)
    {
        // RESET LOGIC. THIS NEEDS TO BE ADDED TO FUTURE INHERITORS.
        transform.DOKill();

        if(transform is RectTransform rectTransform)
            rectTransform.anchoredPosition = defaultPos;
        transform.localScale = defaultScale;
        cg.alpha = 0; // optional reset logic

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
