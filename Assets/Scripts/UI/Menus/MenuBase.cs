using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

// written by andy
// rebuilt as a data driven animator by Claude Opus 5
// the base class for all menu animations
//
// menus used to each hand write their own AnimateIn/AnimateOut sequences plus a pile of
// blackPanelDefPos style fields to remember where everything started. that was the same
// ~120 lines three times over. now the animation is a list of MenuAnimSteps in the inspector
// and this class does the sequencing, the caching and the resetting for everyone.
//
// each menu keeps its own subclass so its original animation can be seeded from a context
// menu -- see MainMenu/SettingsMenu/CreditsMenu.
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
    [SerializeField, Tooltip("plays when the menu opens. values are where things come FROM")]
    protected List<MenuAnimStep> animateInSteps = new();
    [SerializeField, Tooltip("plays when the menu closes. values are where things go TO")]
    protected List<MenuAnimStep> animateOutSteps = new();
    [SerializeField, Tooltip("fallback fade time when a menu has no steps set up yet")]
    protected float fallbackFadeDuration = 0.2f;

    // whatever the animated elements looked like in the scene before anything tweened them.
    // captured automatically so no menu has to keep its own defPos fields any more
    private List<TargetSnapshot> snapshots = new();

    protected GameControls Controls => GameInput.Controls;

    protected virtual void Awake()
    {
        if (cg == null)
        {
            // GetComponentInChildren would happily grab a CanvasGroup from deep inside the
            // menu (on the settings screen it picked up the scroll area's one), and then
            // cg.interactable would be gating on the wrong thing. only accept our own.
            cg = GetComponent<CanvasGroup>();
        }

        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
            Debug.LogWarning($"[MenuBase] >> '{name}' had no CanvasGroup of its own, so one was " +
                $"added. assign it in the inspector to silence this.");
        }

        CacheAnimatedTargets();
    }

    protected virtual void Update()
    {
        if (cg.interactable && Controls.Battle.Back.WasPerformedThisFrame())
        {
            OnBackPressed();
        }
    }

    // note: the old version enabled/disabled the Battle map here. that's gone now the controls
    // are shared -- whichever menu happened to close last would have switched input off for
    // everything else. GameInput keeps the map enabled and cg.interactable does the gating.


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
    /// Override for anything that should only run while the menu is actually up.
    /// </summary>
    public virtual void OnMenuOpened() { }

    /// <summary>
    /// Called by <c>MenuManager</c> as this menu starts closing, before it animates out.
    /// </summary>
    public virtual void OnMenuClosed() { }


    // animation  ===========================================================================

    /// <summary>
    /// Puts every animated element back where the scene had it, so the menu can be replayed
    /// from a clean slate.
    /// </summary>
    public virtual void ResetState(bool resetAlpha = false)
    {
        transform.DOKill();
        cg.DOKill();

        foreach (TargetSnapshot snapshot in snapshots)
        {
            snapshot.Restore();
        }

        if (resetAlpha)
            cg.alpha = 0;
    }

    public virtual void AnimateIn(Action onComplete)
    {
        ResetState();

        cg.gameObject.SetActive(true);

        if (animateInSteps.Count == 0)
        {
            // nothing authored yet -- fall back to the plain fade this class always did
            cg.alpha = 0;
            cg.DOFade(1, fallbackFadeDuration).SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
            return;
        }

        cg.alpha = 1;
        PlaySteps(animateInSteps, isEntering: true, onComplete);
    }

    public virtual void AnimateOut(Action onComplete)
    {
        if (animateOutSteps.Count == 0)
        {
            cg.DOFade(0, fallbackFadeDuration).SetUpdate(true).OnComplete(() =>
            {
                cg.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
            return;
        }

        PlaySteps(animateOutSteps, isEntering: false, onComplete);
    }

    // turns a step list into one sequence. this is the bit that used to be copy pasted into
    // every menu as a wall of sequence.Insert calls
    private void PlaySteps(List<MenuAnimStep> steps, bool isEntering, Action onComplete)
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        foreach (MenuAnimStep step in steps)
        {
            Tween tween = step.CreateTween(isEntering);

            if (tween != null)
            {
                sequence.Insert(step.delay, tween);
            }
        }

        sequence.OnComplete(() => onComplete?.Invoke());
    }


    // target caching  ======================================================================

    // walks both step lists and remembers the starting state of everything they touch.
    // replaces the per menu "blackPanelDefPos = blackPanel.anchoredPosition" bookkeeping
    private void CacheAnimatedTargets()
    {
        snapshots.Clear();

        HashSet<Component> seen = new();

        CacheFrom(animateInSteps, seen);
        CacheFrom(animateOutSteps, seen);
    }

    private void CacheFrom(List<MenuAnimStep> steps, HashSet<Component> seen)
    {
        foreach (MenuAnimStep step in steps)
        {
            if (step.target == null || seen.Contains(step.target))
                continue;

            seen.Add(step.target);
            snapshots.Add(new TargetSnapshot(step.target));
        }
    }

    // everything we need to undo any of the MenuAnimProperty tweens on one object
    private class TargetSnapshot
    {
        private readonly Component target;
        private readonly RectTransform rect;
        private readonly CanvasGroup canvasGroup;
        private readonly Graphic graphic;

        private readonly Vector2 anchoredPosition;
        private readonly Vector3 worldPosition;
        private readonly Vector3 localScale;
        private readonly float alpha;

        public TargetSnapshot(Component target)
        {
            this.target = target;

            rect = target.transform as RectTransform;
            canvasGroup = target as CanvasGroup;
            graphic = target as Graphic;

            if (rect != null)
                anchoredPosition = rect.anchoredPosition;

            worldPosition = target.transform.position;
            localScale = target.transform.localScale;

            if (canvasGroup != null)
                alpha = canvasGroup.alpha;
            else if (graphic != null)
                alpha = graphic.color.a;
            else
                alpha = 1f;
        }

        public void Restore()
        {
            if (target == null)
                return;

            target.transform.DOKill();

            if (rect != null)
                rect.anchoredPosition = anchoredPosition;
            else
                target.transform.position = worldPosition;

            target.transform.localScale = localScale;

            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = alpha;
            }
            else if (graphic != null)
            {
                graphic.DOKill();
                Color color = graphic.color;
                graphic.color = new Color(color.r, color.g, color.b, alpha);
            }
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
