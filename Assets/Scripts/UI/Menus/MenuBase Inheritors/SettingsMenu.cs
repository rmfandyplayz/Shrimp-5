using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

// written by andy
// animations for settings menu
public class SettingsMenu : MenuBase
{
    [SerializeField] Image backgroundImage;
    [SerializeField] RectTransform blackPanel;
    [SerializeField] CanvasGroup scrollCanvasGroup;

    private Vector2 blackPanelDefPos;
    private Vector2 backgroundDefPos;
    private Vector2 scrollDefPos;

    public override void Awake()
    {
        base.Awake();

        blackPanelDefPos = blackPanel.anchoredPosition;
        backgroundDefPos = backgroundImage.rectTransform.anchoredPosition;
        scrollDefPos = scrollCanvasGroup.transform.localPosition;
    }

    public override void AnimateIn(Action onComplete)
    {
        ResetState();

        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0, blackPanel.DOAnchorPosX(-1440, 0.5f).From().SetEase(Ease.OutQuad));

        sequence.Insert(0.2f, backgroundImage.rectTransform.DOAnchorPosY(-50, 0.7f).From().SetEase(Ease.OutQuint));
        sequence.Insert(0.2f, backgroundImage.DOFade(0, 0.45f).From());

        sequence.Insert(0.5f, scrollCanvasGroup.transform.DOMoveX(850, 0.5f).From().SetEase(Ease.OutQuad));
        sequence.Insert(0.5f, scrollCanvasGroup.DOFade(0, 0.5f).From());

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public override void AnimateOut(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0, blackPanel.DOAnchorPosX(-1440, 0.5f).SetEase(Ease.OutQuad));

        sequence.Insert(0f, backgroundImage.rectTransform.DOAnchorPosY(-50, 0.7f).SetEase(Ease.OutQuint));
        sequence.Insert(0f, backgroundImage.DOFade(0, 0.45f));

        sequence.Insert(0f, scrollCanvasGroup.transform.DOMoveX(850, 0.5f).SetEase(Ease.OutQuad));
        sequence.Insert(0f, scrollCanvasGroup.DOFade(0, 0.5f));

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public override void ResetState(bool resetAlpha = false)
    {
        base.ResetState(resetAlpha);

        blackPanel.DOKill();
        backgroundImage.DOKill();
        scrollCanvasGroup.DOKill();

        blackPanel.anchoredPosition = blackPanelDefPos;
        backgroundImage.rectTransform.anchoredPosition = backgroundDefPos;
        scrollCanvasGroup.transform.localPosition = scrollDefPos;

        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1);
        scrollCanvasGroup.alpha = 1;
    }
}
