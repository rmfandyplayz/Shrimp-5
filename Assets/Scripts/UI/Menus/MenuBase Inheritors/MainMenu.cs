using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// written by andy
// name might be confusing, but this is for the main part of the main menu
// where it shows play settings credits exit.
public class MainMenu : MenuBase
{
    [SerializeField] List<TextMeshProUGUI> waterfallElements = new();
    [SerializeField] Image gameLogo;
    [SerializeField] RectTransform blackPanel;
    [SerializeField] Image backgroundArt;

    private Vector2 blackPanelDefPos;
    private Vector2 logoDefPos;
    private Vector2 artDefPos;
    private List<Vector2> waterfallDefPos = new();

    public override void Awake()
    {
        base.Awake();

        blackPanelDefPos = blackPanel.anchoredPosition;
        logoDefPos = gameLogo.rectTransform.anchoredPosition;
        artDefPos = backgroundArt.rectTransform.anchoredPosition;

        foreach (var item in waterfallElements)
        {
            waterfallDefPos.Add(item.rectTransform.anchoredPosition);
        }
    }

    public override void AnimateIn(Action onComplete)
    {
        // sometimes i write code so bad i hope people will never trust my programming
        // skills again and therefore i can just design the ui without programming them

        // however i for some fucking reason still like programming so i'm just screwing
        // myself up i suppose

        ResetState();

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

        sequence.Insert(0.3f, blackPanel.DOAnchorPosX(500, 0.5f).SetEase(Ease.OutQuad));

        sequence.Insert(0.55f, gameLogo.rectTransform.DOAnchorPosX(300, 0.2f).SetEase(Ease.OutCubic));
        sequence.Insert(0.55f, gameLogo.DOFade(0, 0.2f));

        sequence.Insert(0.5f, waterfallElements[0].rectTransform.DOAnchorPosX(300, 0.2f).SetEase(Ease.OutQuad));
        sequence.Insert(0.5f, waterfallElements[0].DOFade(0, 0.2f));
        sequence.Insert(0.45f, waterfallElements[1].rectTransform.DOAnchorPosX(300, 0.2f).SetEase(Ease.OutQuad));
        sequence.Insert(0.45f, waterfallElements[1].DOFade(0, 0.2f));
        sequence.Insert(0.4f, waterfallElements[2].rectTransform.DOAnchorPosX(300, 0.2f).SetEase(Ease.OutQuad));
        sequence.Insert(0.4f, waterfallElements[2].DOFade(0, 0.2f));
        sequence.Insert(0.35f, waterfallElements[3].rectTransform.DOAnchorPosX(300, 0.2f).SetEase(Ease.OutQuad));
        sequence.Insert(0.35f, waterfallElements[3].DOFade(0, 0.2f));

        sequence.Insert(0.35f, backgroundArt.rectTransform.DOAnchorPosY(-50, 0.7f).SetEase(Ease.OutQuint));
        sequence.Insert(0.35f, backgroundArt.DOFade(0, 0.45f));

        sequence.OnComplete(() => onComplete?.Invoke());
    }

    public override void ResetState(bool resetAlpha = false)
    {
        base.ResetState(resetAlpha);

        blackPanel.DOKill();
        gameLogo.DOKill();
        backgroundArt.DOKill();
        foreach (var item in waterfallElements) 
            item.DOKill();

        blackPanel.anchoredPosition = blackPanelDefPos;
        gameLogo.rectTransform.anchoredPosition = logoDefPos;
        backgroundArt.rectTransform.anchoredPosition = artDefPos;

        for (int i = 0; i < waterfallElements.Count; i++)
        {
            waterfallElements[i].rectTransform.anchoredPosition = waterfallDefPos[i];
            waterfallElements[i].alpha = 1; // Reset text alpha
        }

        gameLogo.color = new Color(gameLogo.color.r, gameLogo.color.g, gameLogo.color.b, 1);
        backgroundArt.color = new Color(backgroundArt.color.r, backgroundArt.color.g, backgroundArt.color.b, 1);
    }
}
