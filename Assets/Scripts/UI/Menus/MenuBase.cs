using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Events;

// written by andy
// animation moved out into attachable modules by Claude Opus 5
// the base class for all menus
//
// this no longer knows anything about HOW a menu animates. it collects whatever
// IMenuAnimation modules are attached, tells them to play, and waits for the slowest one --
// the same composition pattern MenuInteractable uses with IMenuFeedback[], just with
// completion callbacks since animations take time.
//
// so: to give a menu an animation, add a MenuAnim_Transition to it. to take it away, remove
// the component. to invent a new kind, write a class implementing IMenuAnimation. nothing in
// here changes either way.
public class MenuBase : MonoBehaviour
{
    [SerializeField]
    [Tooltip("leave blank if it doesn't apply!")]
    protected MenuBase backMenu; // menu will go back to this
    [SerializeField]
    [Tooltip("the first selected element when opening this menu")]
    protected GameObject firstSelected; // the first selected element when opening this menu

    [SerializeField] protected CanvasGroup cg;

    [Header("animation")]
    [SerializeField, Tooltip("fade time used only when no IMenuAnimation modules are attached")]
    protected float fallbackFadeDuration = 0.2f;

    [Header("events")]
    [SerializeField, Tooltip("fires once this menu has finished opening and is taking input")]
    private UnityEvent onMenuOpenedEvent;
    [SerializeField, Tooltip("fires as this menu starts closing, before it animates out")]
    private UnityEvent onMenuClosedEvent;

    // everything attached that wants to animate when this menu opens or closes
    private IMenuAnimation[] animations;

    protected GameControls Controls => GameInput.Controls;

    protected virtual void Awake()
    {
        if (cg == null)
        {
            // deliberately GetComponent and not GetComponentInChildren -- the latter would
            // happily grab a CanvasGroup from deep inside the menu (on the settings screen it
            // picked up the scroll area's), and then cg.interactable gated the wrong thing
            cg = GetComponent<CanvasGroup>();
        }

        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
            Debug.LogWarning($"[MenuBase] >> '{name}' had no CanvasGroup of its own, so one was " +
                $"added. assign it in the inspector to silence this.");
        }

        // includeInactive so a module sitting on a hidden child still gets found
        animations = GetComponentsInChildren<IMenuAnimation>(includeInactive: true);
    }

    protected virtual void Update()
    {
        if (cg.interactable && Controls.Battle.Back.WasPerformedThisFrame())
        {
            OnBackPressed();
        }
    }

    // note: the battle map used to get enabled/disabled here. that's gone now the controls are
    // shared -- whichever menu closed last would have switched input off for everything else.
    // GameInput keeps the map enabled and cg.interactable does the gating.


    // getters vvvvvvv (wow i'm being such a goody two shoes by following what CS1420 taught me)

    public CanvasGroup GetCanvasGroup()
    {
        return cg;
    }

    public GameObject GetFirstSelected()
    {
        return firstSelected;
    }


    // menu lifecycle hooks  ================================================================

    /// <summary>
    /// Called by <c>MenuManager</c> once this menu has finished animating in and is taking input.
    /// </summary>
    public virtual void OnMenuOpened()
    {
        onMenuOpenedEvent?.Invoke();
    }

    /// <summary>
    /// Called by <c>MenuManager</c> as this menu starts closing, before it animates out.
    /// </summary>
    public virtual void OnMenuClosed()
    {
        onMenuClosedEvent?.Invoke();
    }


    // animation  ===========================================================================

    /// <summary>
    /// Puts every animated element back where the scene had it, so the menu can be replayed
    /// from a clean slate.
    /// </summary>
    public virtual void ResetState(bool resetAlpha = false)
    {
        transform.DOKill();
        cg.DOKill();

        // can be called before Awake has run if something resets a menu very early
        if (animations != null)
        {
            foreach (IMenuAnimation animation in animations)
            {
                animation.ResetState();
            }
        }

        if (resetAlpha)
            cg.alpha = 0;
    }

    public virtual void AnimateIn(Action onComplete)
    {
        ResetState();

        cg.gameObject.SetActive(true);

        if (animations.Length == 0)
        {
            // no modules attached -- fall back to the plain fade this class always did, so a
            // menu without an animation still appears rather than popping in
            cg.alpha = 0;
            cg.DOFade(1, fallbackFadeDuration).SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
            return;
        }

        cg.alpha = 1;
        PlayAll(isEntering: true, onComplete);
    }

    public virtual void AnimateOut(Action onComplete)
    {
        if (animations.Length == 0)
        {
            cg.DOFade(0, fallbackFadeDuration).SetUpdate(true).OnComplete(() =>
            {
                cg.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
            return;
        }

        PlayAll(isEntering: false, onComplete);
    }

    // runs every module and fires onComplete when the LAST one reports back.
    // counted rather than timed, because modules can't know each other's length
    private void PlayAll(bool isEntering, Action onComplete)
    {
        int remaining = animations.Length;

        void ReportDone()
        {
            remaining--;

            if (remaining <= 0)
                onComplete?.Invoke();
        }

        foreach (IMenuAnimation animation in animations)
        {
            if (isEntering)
                animation.PlayIn(ReportDone);
            else
                animation.PlayOut(ReportDone);
        }
    }


    // input  ===============================================================================

    public virtual void OnBackPressed()
    {
        if (backMenu != null)
        {
            MenuManager.Instance.SwitchMenu(backMenu);
        }
    }
}
