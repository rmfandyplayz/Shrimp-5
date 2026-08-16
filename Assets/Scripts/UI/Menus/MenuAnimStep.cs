using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// written by Claude Opus 5
// one tween in a menu's open/close animation, as data instead of code.
//
// every animation across the three menus was some combination of "slide this on an axis",
// "fade this", and "scale this", at a delay, for a duration, with an ease. so that's what this
// is. MenuBase turns a list of these into the DOTween Sequence the menus used to hand write.

/// <summary>
/// What a <c>MenuAnimStep</c> animates.
/// </summary>
public enum MenuAnimProperty
{
    AnchorPosX,
    AnchorPosY,
    /// <summary>
    /// World space X, i.e. <c>DOMoveX</c>. Only here because the settings menu's scroll panel
    /// was written that way and switching it to anchored would move it somewhere else.
    /// Prefer <c>AnchorPosX</c> for anything new.
    /// </summary>
    WorldMoveX,
    Fade,
    Scale
}

[Serializable]
public class MenuAnimStep
{
    [Tooltip("just for reading the list in the inspector. doesn't do anything")]
    public string label;

    [Tooltip("what moves. a RectTransform for position/scale, a Graphic or CanvasGroup to fade")]
    public Component target;

    public MenuAnimProperty property;

    [Tooltip("for AnimateIn this is where it comes FROM. for AnimateOut it's where it goes TO")]
    public float value;

    [Tooltip("seconds into the sequence before this starts")]
    public float delay;

    public float duration = 0.5f;

    public Ease ease = Ease.OutQuad;

    /// <summary>
    /// Builds the tween for this step.
    ///
    /// <paramref name="isEntering"/> flips it between the two forms every menu used by hand:
    /// entering animates <c>.From()</c> the value to wherever the element sits in the scene,
    /// leaving animates from where it sits out to the value.
    /// </summary>
    public Tween CreateTween(bool isEntering)
    {
        // has to stay a Tweener rather than a Tween -- .From() is only defined on Tweener
        Tweener tween = CreateBaseTween();

        if (tween == null)
            return null;

        if (isEntering)
        {
            tween = tween.From();
        }

        return tween.SetEase(ease).SetUpdate(true);
    }

    private Tweener CreateBaseTween()
    {
        if (target == null)
        {
            Debug.LogWarning($"[MenuAnimStep] >> step '{label}' has no target assigned.");
            return null;
        }

        switch (property)
        {
            case MenuAnimProperty.AnchorPosX:
                return GetRect()?.DOAnchorPosX(value, duration);

            case MenuAnimProperty.AnchorPosY:
                return GetRect()?.DOAnchorPosY(value, duration);

            case MenuAnimProperty.WorldMoveX:
                return target.transform.DOMoveX(value, duration);

            case MenuAnimProperty.Scale:
                return target.transform.DOScale(value, duration);

            case MenuAnimProperty.Fade:
                return CreateFadeTween();

            default:
                return null;
        }
    }

    // fading works on either a CanvasGroup or any Graphic (Image, TextMeshProUGUI, ...)
    private Tweener CreateFadeTween()
    {
        if (target is CanvasGroup canvasGroup)
            return canvasGroup.DOFade(value, duration);

        if (target is Graphic graphic)
            return graphic.DOFade(value, duration);

        // TextMeshProUGUI is a Graphic, so it's already covered above. anything else isn't
        // fadeable and is almost certainly a mis-drag in the inspector
        Debug.LogWarning($"[MenuAnimStep] >> step '{label}' wants to fade a " +
            $"{target.GetType().Name}, which has no alpha. assign a CanvasGroup or a Graphic.");
        return null;
    }

    private RectTransform GetRect()
    {
        if (target is RectTransform rect)
            return rect;

        if (target.transform is RectTransform ownRect)
            return ownRect;

        Debug.LogWarning($"[MenuAnimStep] >> step '{label}' needs a RectTransform to move.");
        return null;
    }
}
