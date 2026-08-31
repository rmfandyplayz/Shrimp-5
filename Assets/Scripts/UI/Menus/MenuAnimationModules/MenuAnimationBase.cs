using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// written by Claude Opus 5
// shared plumbing for menu animation modules.
//
// every module needs the same two boring things: a list of what it animates, and a memory of
// where those things started so it can put them back. that lives here so an actual module only
// has to describe its animation.
//
// this used to be MenuBase's job, which meant MenuBase had to know about every property any
// menu might ever animate. now a module cleans up after itself.
public abstract class MenuAnimationBase : MonoBehaviour, IMenuAnimation
{
    [Header("what does this animate?")]
    [SerializeField, Tooltip("note: will use the gameobject this is attached to if empty")]
    protected List<Component> targets = new();

    // where everything sat in the scene before anything tweened it
    private List<TargetSnapshot> snapshots = new();
    private List<Component> resolvedTargets = new();

    protected virtual void Awake()
    {
        ResolveTargets();
        CacheTargets();
    }

    /// <summary>
    /// The things this module animates, in order. Empty <c>targets</c> means "just me".
    /// </summary>
    protected IReadOnlyList<Component> Targets => resolvedTargets;

    public abstract void PlayIn(Action onComplete);

    public abstract void PlayOut(Action onComplete);

    public virtual void ResetState()
    {
        foreach (TargetSnapshot snapshot in snapshots)
        {
            snapshot.Restore();
        }
    }

    private void ResolveTargets()
    {
        resolvedTargets.Clear();

        foreach (Component target in targets)
        {
            if (target != null)
                resolvedTargets.Add(target);
        }

        // nothing assigned -> animate whatever we're sitting on, matching how
        // UIFeedback_Scale treats an empty target field
        if (resolvedTargets.Count == 0)
        {
            // a list that WAS filled in but resolved to nothing means broken references, not
            // "animate me". silently animating the menu root instead would look like the
            // animation itself is wrong, so say so.
            if (targets.Count > 0)
            {
                Debug.LogWarning($"[{GetType().Name}] >> '{name}' has {targets.Count} target " +
                    $"slot(s) but they're all empty. falling back to animating this object.");
            }

            resolvedTargets.Add(transform);
        }
    }

    private void CacheTargets()
    {
        snapshots.Clear();

        foreach (Component target in resolvedTargets)
        {
            snapshots.Add(new TargetSnapshot(target));
        }
    }

    // everything needed to undo a move/fade/scale on one object
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
}
