using TMPro;
using UnityEngine;
using UnityEngine.UI;

// written by Claude Opus 5
// one little icon in a shrimp's status effect row. shows what the status is and how many
// turns are left on it.
public class StatusIcon : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private Image icon;
    [SerializeField, Tooltip("optional. hidden for permanent statuses")]
    private TextMeshProUGUI turnsText;

    [Header("tint by whether the status is good or bad")]
    [SerializeField] private bool tintByEffectType = true;
    [SerializeField] private Color positiveColor = Color.cyan;
    [SerializeField] private Color negativeColor = Color.magenta;

    private AppliedStatus boundStatus;

    /// <summary>
    /// Points this icon at an applied status. Pass null to blank it out.
    /// </summary>
    public void Bind(AppliedStatus applied)
    {
        boundStatus = applied;

        if (applied == null || applied.status == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (icon != null)
        {
            icon.sprite = UISprites.Get(applied.status.iconID);

            if (tintByEffectType)
            {
                icon.color = applied.status.effectType == TypeOfEffect.Positive
                    ? positiveColor
                    : negativeColor;
            }
        }

        RefreshTurns();
    }

    /// <summary>
    /// Re-reads the remaining turn count off the bound status. Cheaper than a full rebind
    /// when only the counter ticked down.
    /// </summary>
    public void RefreshTurns()
    {
        if (turnsText == null)
            return;

        if (boundStatus == null || boundStatus.status == null)
        {
            turnsText.gameObject.SetActive(false);
            return;
        }

        // note the spelling on the definition field is "permanant"
        bool showCounter = !boundStatus.status.permanant;

        turnsText.gameObject.SetActive(showCounter);

        if (showCounter)
        {
            turnsText.text = boundStatus.remainingTurns.ToString();
        }
    }

    /// <summary>
    /// Whatever this icon is currently showing, or null if it's been cleared.
    /// </summary>
    public AppliedStatus GetBoundStatus()
    {
        return boundStatus;
    }

    /// <summary>
    /// Whether this icon is the one showing the given status.
    ///
    /// Heads up: <c>statusID</c> is blank on every status asset right now, so this matches
    /// empty against empty and can pick the wrong icon when a shrimp has several.
    /// </summary>
    public bool Represents(string statusId)
    {
        if (boundStatus == null || boundStatus.status == null)
            return false;

        return boundStatus.status.statusID == statusId;
    }

    /// <summary>
    /// Unbinds and hides. The object stays in the pool to be reused.
    /// </summary>
    public void Clear()
    {
        boundStatus = null;
        gameObject.SetActive(false);
    }
}
