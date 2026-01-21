using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MenuBase
{
    [SerializeField] Image backgroundImage;
    [SerializeField] Image blackPanel;
    [SerializeField] CanvasGroup scrollCanvasGroup;

    public override void AnimateIn(Action onComplete)
    {
        ResetState();

        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0, blackPanel.rectTransform.DOAnchorPosX(-1440, 0.5f).From().SetEase(Ease.OutQuad));

        sequence.Insert(0.2f, backgroundImage.rectTransform.DOAnchorPosY(-50, 0.7f).From().SetEase(Ease.OutQuint));
        sequence.Insert(0.2f, backgroundImage.DOFade(0, 0.45f).From());

        sequence.Insert(0.5f, scrollCanvasGroup.transform.DOMoveX(850, 0.5f).From().SetEase(Ease.OutQuad));
        sequence.Insert(0.5f, scrollCanvasGroup.DOFade(0, 0.5f).From());

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public override void AnimateOut(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0, blackPanel.rectTransform.DOAnchorPosX(-1440, 0.5f).SetEase(Ease.OutQuad));

        sequence.Insert(0f, backgroundImage.rectTransform.DOAnchorPosY(-50, 0.7f).SetEase(Ease.OutQuint));
        sequence.Insert(0f, backgroundImage.DOFade(0, 0.45f));

        sequence.Insert(0f, scrollCanvasGroup.transform.DOMoveX(850, 0.5f).SetEase(Ease.OutQuad));
        sequence.Insert(0f, scrollCanvasGroup.DOFade(0, 0.5f));

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public override void OnBackPressed()
    {
        if (backMenu != null)
        {
            MenuManager.Instance.SwitchMenu(backMenu);
        }
    }
}
