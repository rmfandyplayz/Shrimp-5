using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// written by Claude Opus 5
// the hp bar plus the "12 / 40" text sitting on top of it.
//
// SetValue snaps for now. when we do the DOTween pass, the tween goes inside SetValue and
// fires onComplete when it lands -- nothing that calls this needs to change.
public class HealthBar : MonoBehaviour
{
    [Header("refs")]
    [SerializeField, Tooltip("image type must be Filled for the bar to actually move")]
    private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("settings")]
    [SerializeField, Tooltip("colour when hp is healthy")]
    private Color healthyColor = Color.green;
    [SerializeField, Tooltip("colour when hp drops below the danger threshold")]
    private Color dangerColor = Color.red;
    [SerializeField, Range(0f, 1f)]
    private float dangerThreshold = 0.25f;
    [SerializeField, Tooltip("turn off if you'd rather set the bar colour yourself")]
    private bool tintByHealth = true;

    private int currentValue;
    private int maxValue;

    /// <summary>
    /// Moves the bar to <paramref name="current"/> out of <paramref name="max"/>.
    ///
    /// <paramref name="onComplete"/> fires when the bar has finished moving. Right now that's
    /// immediately since there's no animation yet, but handlers already wait on it so the
    /// DOTween version will just work.
    /// </summary>
    public void SetValue(int current, int max, Action onComplete = null)
    {
        maxValue = Mathf.Max(max, 1); // never divide by zero
        currentValue = Mathf.Clamp(current, 0, maxValue);

        // TODO (animation): tween fillImage.fillAmount from its current value to this one,
        // and count healthText up/down alongside it, then invoke onComplete on finish
        ApplyFill(GetFillAmount());
        ApplyText();

        onComplete?.Invoke();
    }

    /// <summary>
    /// Sets the bar without any animation, even once animations exist.
    /// Use this when binding a shrimp for the first time.
    /// </summary>
    public void SetValueInstant(int current, int max)
    {
        maxValue = Mathf.Max(max, 1);
        currentValue = Mathf.Clamp(current, 0, maxValue);

        ApplyFill(GetFillAmount());
        ApplyText();
    }

    /// <summary>
    /// Current hp as a 0..1 fraction, which is what the bar's fillAmount wants.
    /// </summary>
    public float GetFillAmount()
    {
        return (float)currentValue / maxValue;
    }

    // fillImage has to be set to Image Type "Filled" in the inspector or this does nothing
    private void ApplyFill(float fillAmount)
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount = fillAmount;

        if (tintByHealth)
        {
            fillImage.color = fillAmount <= dangerThreshold ? dangerColor : healthyColor;
        }
    }

    private void ApplyText()
    {
        if (healthText == null)
            return;

        healthText.text = $"{currentValue} / {maxValue}";
    }
}
