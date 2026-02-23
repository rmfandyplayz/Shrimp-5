using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIFeedback_ChangeBG : MonoBehaviour, IMenuFeedback
{
    [SerializeField] Image targetImage;
    [SerializeField] Sprite desiredImage;

    private Vector2 targetImageDefPos;

    private void Awake()
    {
        targetImageDefPos = targetImage.rectTransform.anchoredPosition;
    }

    public void OnSelect()
    {
        if (targetImage.sprite == desiredImage)
            return;

        ResetState();


        targetImage.sprite = desiredImage;

        targetImage.rectTransform.DOAnchorPosY(-50, 0.7f).From().SetEase(Ease.OutQuint);
        targetImage.DOFade(1, 0.45f);
    }

    public void OnDeselect() { }

    public void OnSubmit() { }
    
    private void ResetState()
    {
        targetImage.rectTransform.DOKill();
        targetImage.DOKill();

        targetImage.color = new Color(1, 1, 1, 0);
        targetImage.rectTransform.anchoredPosition = targetImageDefPos;
    }
}
