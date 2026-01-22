using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CreditsMenu : MenuBase
{
    [SerializeField] TextMeshProUGUI settingsText;
    [SerializeField] TextMeshProUGUI soundCreditsText;
    [SerializeField] CanvasGroup andyGroup;
    [SerializeField] CanvasGroup cassandraGroup;
    [SerializeField] CanvasGroup owenGroup;
    [SerializeField] CanvasGroup soundCreditsList;

    private Vector2 settingsTxtDefPos;
    private Vector2 soundCreditsTxtDefPos;
    private Vector2 andyGroupDefPos;
    private Vector2 cassandraGroupDefPos;
    private Vector2 owenGroupDefPos;
    private Vector2 soundCreditsListDefPos;

    public override void Awake()
    {
        base.Awake();

        settingsTxtDefPos = settingsText.rectTransform.anchoredPosition;
        soundCreditsTxtDefPos = soundCreditsText.rectTransform.anchoredPosition;
        andyGroupDefPos = andyGroup.transform.localPosition;
        cassandraGroupDefPos = cassandraGroup.transform.localPosition;
        owenGroupDefPos = owenGroup.transform.localPosition;
        soundCreditsListDefPos = soundCreditsList.transform.localPosition;
    }

    public override void AnimateIn(Action onComplete)
    {
        ResetState();

        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0, settingsText.DOFade(0, 0.6f).From());
        sequence.Insert(0, settingsText.rectTransform.DOScale(3.5f, 0.6f).From().SetEase(Ease.OutBack));

        sequence.Insert(0.05f, andyGroup.DOFade(0, 0.6f).From());
        sequence.Insert(0.05f, andyGroup.transform.DOScale(3.5f, 0.6f).From().SetEase(Ease.OutBack));

        sequence.Insert(0.1f, cassandraGroup.DOFade(0, 0.6f).From());
        sequence.Insert(0.1f, cassandraGroup.transform.DOScale(3.5f, 0.6f).From().SetEase(Ease.OutBack));

        sequence.Insert(0.15f, owenGroup.DOFade(0, 0.6f).From());
        sequence.Insert(0.15f, owenGroup.transform.DOScale(3.5f, 0.6f).From().SetEase(Ease.OutBack));

        sequence.Insert(0.2f, soundCreditsText.DOFade(0, 0.6f).From());
        sequence.Insert(0.2f, soundCreditsText.rectTransform.DOScale(3.5f, 0.6f).From().SetEase(Ease.OutBack));

        sequence.Insert(0.25f, soundCreditsList.DOFade(0, 0.6f).From());
        sequence.Insert(0.25f, soundCreditsList.transform.DOScale(2f, 0.6f).From().SetEase(Ease.OutBack));

        sequence.OnComplete(() => onComplete.Invoke());
    }

    public override void AnimateOut(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Insert(0.25f, settingsText.DOFade(0, 0.6f));
        sequence.Insert(0.25f, settingsText.rectTransform.DOScale(2f, 0.6f).SetEase(Ease.InQuart));

        sequence.Insert(0.2f, andyGroup.DOFade(0, 0.6f));
        sequence.Insert(0.2f, andyGroup.transform.DOScale(2f, 0.6f).SetEase(Ease.InQuart));

        sequence.Insert(0.15f, cassandraGroup.DOFade(0, 0.6f));
        sequence.Insert(0.15f, cassandraGroup.transform.DOScale(2f, 0.6f).SetEase(Ease.InQuart));

        sequence.Insert(0.1f, owenGroup.DOFade(0, 0.6f));
        sequence.Insert(0.1f, owenGroup.transform.DOScale(2f, 0.6f).SetEase(Ease.InQuart));

        sequence.Insert(0.05f, soundCreditsText.DOFade(0, 0.6f));
        sequence.Insert(0.05f, soundCreditsText.rectTransform.DOScale(2f, 0.6f).SetEase(Ease.InQuart));

        sequence.Insert(0f, soundCreditsList.DOFade(0, 0.6f));
        sequence.Insert(0f, soundCreditsList.transform.DOScale(2f, 0.6f).SetEase(Ease.InQuart));

        sequence.OnComplete(() => onComplete.Invoke());
    }


    public override void ResetState(bool resetAlpha = false)
    {
        base.ResetState(resetAlpha);

        settingsText.DOKill();
        soundCreditsText.DOKill();
        andyGroup.DOKill();
        cassandraGroup.DOKill();
        owenGroup.DOKill();
        soundCreditsList.DOKill();

        settingsText.rectTransform.anchoredPosition = settingsTxtDefPos;
        soundCreditsText.rectTransform.anchoredPosition = soundCreditsTxtDefPos;
        andyGroup.transform.localPosition = andyGroupDefPos;
        cassandraGroup.transform.localPosition = cassandraGroupDefPos;
        owenGroup.transform.localPosition = owenGroupDefPos;
        soundCreditsList.transform.localPosition = soundCreditsListDefPos;

        settingsText.alpha = 1;
        soundCreditsText.alpha = 1;
        andyGroup.alpha = 1;
        cassandraGroup.alpha = 1;
        owenGroup.alpha = 1;
        soundCreditsList.alpha = 1;

        settingsText.rectTransform.localScale = Vector3.one;
        soundCreditsText.rectTransform.localScale = Vector3.one;
        andyGroup.transform.localScale = Vector3.one;
        cassandraGroup.transform.localScale = Vector3.one;
        owenGroup.transform.localScale = Vector3.one;
        soundCreditsList.transform.localScale = Vector3.one;
    }
}
