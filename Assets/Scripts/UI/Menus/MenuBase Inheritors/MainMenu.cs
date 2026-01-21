using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditorInternal.ReorderableList;

// written by andy
// name might be confusing, but this is for the main part of the main menu
// where it shows play settings credits exit.
public class MainMenu : MenuBase
{
    [SerializeField] List<TextMeshProUGUI> waterfallElements = new();
    [SerializeField] Image gameLogo;
    [SerializeField] RectTransform blackPanel;
    [SerializeField] Image backgroundArt;

    public override void AnimateIn(Action onComplete)
    {
        // sometimes i write code so bad i hope people will never trust my programming
        // skills again and therefore i can just design the ui without programming them

        // however i for some fucking reason still like programming so i'm just screwing
        // myself up i suppose

        transform.DOKill();

        if (transform is RectTransform rectTransform)
            rectTransform.anchoredPosition = defaultPos;
        transform.localScale = defaultScale;
        //cg.alpha = 0;

        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0f, blackPanel.DOAnchorPosX(500, 0.5f).From().SetEase(Ease.OutQuad));

        sequence.Insert(0.4f, gameLogo.rectTransform.DOAnchorPosX(300, 0.35f).From().SetEase(Ease.OutCubic));
        sequence.Insert(0.4f, gameLogo.DOFade(0, 0.35f).From());

        sequence.Insert(0.5f, waterfallElements[0].rectTransform.DOAnchorPosX(300, 0.5f).From().SetEase(Ease.OutQuad));
        sequence.Insert(0.5f, waterfallElements[0].DOFade(0, 0.5f).From());
        sequence.Insert(0.6f, waterfallElements[1].rectTransform.DOAnchorPosX(300, 0.5f).From().SetEase(Ease.OutQuad));
        sequence.Insert(0.6f, waterfallElements[1].DOFade(0, 0.5f).From());
        sequence.Insert(0.7f, waterfallElements[2].rectTransform.DOAnchorPosX(300, 0.5f).From().SetEase(Ease.OutQuad));
        sequence.Insert(0.7f, waterfallElements[2].DOFade(0, 0.5f).From());
        sequence.Insert(0.8f, waterfallElements[3].rectTransform.DOAnchorPosX(300, 0.5f).From().SetEase(Ease.OutQuad));
        sequence.Insert(0.8f, waterfallElements[3].DOFade(0, 0.5f).From());

        sequence.Insert(0.2f, backgroundArt.rectTransform.DOAnchorPosY(-50, 0.7f).From().SetEase(Ease.OutQuint));
        sequence.Insert(0.2f, backgroundArt.DOFade(0, 0.45f).From());

        sequence.OnComplete(() => onComplete?.Invoke());
    }

    public override void AnimateOut(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0.5f, cg.transform.DOMoveX(2046, 0.7f).SetEase(Ease.InQuint));

        sequence.OnComplete(() => onComplete?.Invoke());
    }
}
