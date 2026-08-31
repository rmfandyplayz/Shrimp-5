using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// written by Claude Opus 5
// the general purpose menu transition -- think a google slides transition applied to a group
// of elements. slides, fades and scales them, together, with an optional stagger between them.
//
// grouped by TIMING, not by property: one of these is "these things animate together, like
// this". an element that slides and fades at the same moment is one module with two channels
// on, not two modules that have to be kept in sync.
//
// the target list is what makes the stagger useful -- the main menu's four button waterfall is
// a single module with four targets and stagger 0.1. a negative stagger runs the cascade
// backwards, which is how the exit animation reverses it.
public class MenuAnim_Transition : MenuAnimationBase
{
    [Tooltip("just so you can tell several of these apart on one object. does nothing")]
    public string label;

    [Header("opening")]
    [SerializeField] private MenuTransitionSettings inSettings = new();

    [Header("closing")]
    [SerializeField] private MenuTransitionSettings outSettings = new();

    public override void PlayIn(Action onComplete)
    {
        Play(inSettings, isEntering: true, onComplete);
    }

    public override void PlayOut(Action onComplete)
    {
        Play(outSettings, isEntering: false, onComplete);
    }

    /// <summary>
    /// Points this module at a set of targets with the given timings.
    /// Used by the menus' migration buttons; you'd normally just fill this in the inspector.
    /// </summary>
    public void Configure(string newLabel, IEnumerable<Component> newTargets,
        MenuTransitionSettings opening, MenuTransitionSettings closing)
    {
        label = newLabel;
        targets = new List<Component>(newTargets);
        inSettings = opening;
        outSettings = closing;
    }

    // builds one sequence covering every target and every enabled channel.
    //
    // entering tweens .From(value) up to wherever the element sits in the scene; leaving tweens
    // from where it sits out to value. so `value` always means the hidden/offscreen state, in
    // both directions -- exactly how the hand written sequences were structured.
    private void Play(MenuTransitionSettings settings, bool isEntering, Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();
        int tweenCount = 0;

        for (int i = 0; i < Targets.Count; i++)
        {
            Component target = Targets[i];

            if (target == null)
                continue;

            // stagger can be negative, which walks the delay backwards down the list
            float delay = Mathf.Max(0f, settings.delay + (i * settings.stagger));

            if (settings.move.enabled)
                tweenCount += Insert(sequence, delay, BuildMove(target, settings.move), isEntering, settings.move.ease);

            if (settings.fade.enabled)
                tweenCount += Insert(sequence, delay, BuildFade(target, settings.fade), isEntering, settings.fade.ease);

            if (settings.scale.enabled)
                tweenCount += Insert(sequence, delay, BuildScale(target, settings.scale), isEntering, settings.scale.ease);
        }

        if (tweenCount == 0)
        {
            // nothing enabled. don't leave an empty sequence lying around, and don't stall the
            // menu waiting for a callback that a killed sequence would never fire
            sequence.Kill();
            onComplete?.Invoke();
            return;
        }

        sequence.OnComplete(() => onComplete?.Invoke());
    }

    private int Insert(Sequence sequence, float delay, Tweener tween, bool isEntering, Ease ease)
    {
        if (tween == null)
            return 0;

        // .From() is only defined on Tweener, which is why these builders don't return Tween
        if (isEntering)
            tween = tween.From();

        // SetUpdate(true) so menus keep animating if something has paused the game
        sequence.Insert(delay, tween.SetEase(ease).SetUpdate(true));
        return 1;
    }

    private Tweener BuildMove(Component target, MenuMoveChannel channel)
    {
        if (channel.space == MenuMoveSpace.World)
        {
            return channel.axis == MenuMoveAxis.X
                ? target.transform.DOMoveX(channel.value, channel.duration)
                : target.transform.DOMoveY(channel.value, channel.duration);
        }

        if (target.transform is not RectTransform rect)
        {
            Debug.LogWarning($"[MenuAnim_Transition] >> '{name}' wants to move {target.name} in " +
                $"anchored space but it has no RectTransform. use World space instead.");
            return null;
        }

        return channel.axis == MenuMoveAxis.X
            ? rect.DOAnchorPosX(channel.value, channel.duration)
            : rect.DOAnchorPosY(channel.value, channel.duration);
    }

    private Tweener BuildFade(Component target, MenuFloatChannel channel)
    {
        if (target is CanvasGroup canvasGroup)
            return canvasGroup.DOFade(channel.value, channel.duration);

        // TextMeshProUGUI and Image are both Graphics, so this covers them
        if (target is Graphic graphic)
            return graphic.DOFade(channel.value, channel.duration);

        Debug.LogWarning($"[MenuAnim_Transition] >> '{name}' wants to fade {target.name}, which " +
            $"has no alpha. point it at a CanvasGroup or a Graphic.");
        return null;
    }

    private Tweener BuildScale(Component target, MenuFloatChannel channel)
    {
        return target.transform.DOScale(channel.value, channel.duration);
    }
}


// ---- settings ----------------------------------------------------------------------------

public enum MenuMoveAxis { X, Y }

/// <summary>
/// Anchored is what you almost always want for UI. World exists because the settings menu's
/// scroll panel was written with DOMoveX and an anchored version lands somewhere else.
/// </summary>
public enum MenuMoveSpace { Anchored, World }

[Serializable]
public class MenuMoveChannel
{
    public bool enabled;
    public MenuMoveAxis axis = MenuMoveAxis.X;
    public MenuMoveSpace space = MenuMoveSpace.Anchored;
    [Tooltip("the hidden/offscreen position. slides FROM here when opening, TO here when closing")]
    public float value;
    public float duration = 0.5f;
    public Ease ease = Ease.OutQuad;
}

[Serializable]
public class MenuFloatChannel
{
    public bool enabled;
    [Tooltip("the hidden value. animates FROM here when opening, TO here when closing")]
    public float value;
    public float duration = 0.5f;
    public Ease ease = Ease.OutQuad;
}

/// <summary>
/// One direction's worth of timing. A module has two of these so opening and closing can
/// differ -- the main menu's exit is faster and cascades the other way.
/// </summary>
[Serializable]
public class MenuTransitionSettings
{
    [Tooltip("seconds before the first target starts")]
    public float delay;
    [Tooltip("extra delay per target down the list. negative runs the cascade backwards")]
    public float stagger;

    public MenuMoveChannel move = new();
    public MenuFloatChannel fade = new();
    public MenuFloatChannel scale = new();
}
