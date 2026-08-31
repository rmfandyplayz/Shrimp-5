using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIFeedback_Scale : MonoBehaviour, IMenuFeedback
{
    [SerializeField, Tooltip("note: will use the transform of same gameobject if empty")] Transform target;
    [SerializeField] float scaleAmount;
    [SerializeField] float scaleDuration;
    [SerializeField] Ease selectEasing;
    [SerializeField] Ease deselectEasing;
    [SerializeField] bool affectOthers; // should the scaling of this element affect scaling of other elements in the same layout group?

    private Vector3 originalScale;
    private RectTransform parentLayoutGroup;

    private void Awake()
    {
        if(target == null)
        {
            target = transform; // assign the attached transform as default
        }
        originalScale = target.transform.localScale;

        if(affectOthers == true && transform.parent != null)
        {
            parentLayoutGroup = target.transform.parent.GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        target.transform.localScale = originalScale;
    }

    void IMenuFeedback.OnDeselect()
    {
        target.DOKill();
        target.transform.DOScale(originalScale, scaleDuration).SetUpdate(true).SetEase(deselectEasing).OnUpdate(() =>
        {
            if(affectOthers == true && parentLayoutGroup != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(parentLayoutGroup);
            }
        });
    }

    void IMenuFeedback.OnSelect()
    {
        target.DOKill();
        target.transform.DOScale(scaleAmount, scaleDuration).SetUpdate(true).SetEase(selectEasing).OnUpdate(() =>
        {
            if (affectOthers == true && parentLayoutGroup != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(parentLayoutGroup);
            }
        });
    }

    void IMenuFeedback.OnSubmit()
    {
        Sequence sequence = DOTween.Sequence();

        target.DOKill();
        sequence.Insert(0f, target.DOScale(scaleAmount * 1.2f, 0.1f).SetEase(Ease.OutExpo)).OnUpdate(() =>
        {
            if (affectOthers == true && parentLayoutGroup != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(parentLayoutGroup);
            }
        });
        sequence.Insert(.1f, target.DOScale(scaleAmount, 0.4f).SetEase(Ease.OutCubic)).OnUpdate(() =>
        {
            if (affectOthers == true && parentLayoutGroup != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(parentLayoutGroup);
            }
        });
    }
}
