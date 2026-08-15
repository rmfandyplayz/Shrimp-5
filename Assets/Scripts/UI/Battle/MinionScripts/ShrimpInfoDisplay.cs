using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// written by Claude Opus 5
// the panel showing one shrimp's info -- name, pfp, hp, speed, attack, statuses.
// there's one of these for the player's active shrimp and one for the enemy's.
//
// it knows nothing about BattleEvents. handlers read the event and call the setters here.
//
// every setter takes an optional onComplete so handlers can wait for it. right now they all
// fire straight away since nothing's animated yet -- when the DOTween pass happens, the tween
// goes inside the setter and calls onComplete when it lands. no handler changes needed.
public class ShrimpInfoDisplay : MonoBehaviour
{
    [Header("which side is this display for?")]
    [SerializeField] private BattleSide side;

    [Header("identity refs")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField, Tooltip("the shrimp's profile picture")]
    private Image portrait;

    [Header("stat refs")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI attackText;

    [Header("status refs")]
    [SerializeField] private StatusIconList statusIcons;

    [Header("misc")]
    [SerializeField, Tooltip("optional. hidden when there's no shrimp bound")]
    private CanvasGroup canvasGroup;
    [SerializeField, Tooltip("optional. the root that gets nudged around for hit/heal reactions")]
    private RectTransform reactionRoot;

    // who's currently on show
    private UIShrimpState boundShrimp;

    /// <summary>
    /// Which team this display is for. Set in the inspector, not derived from what's bound.
    /// </summary>
    public BattleSide GetSide()
    {
        return side;
    }

    /// <summary>
    /// The shrimp currently on show, or null if nothing's bound.
    /// </summary>
    public UIShrimpState GetBoundShrimp()
    {
        return boundShrimp;
    }

    /// <summary>
    /// The id of the shrimp on show. <c>InfoDisplayGroup</c> uses this to route events to the
    /// right display without having to parse the id.
    /// </summary>
    public string GetBoundShrimpId()
    {
        return boundShrimp != null ? boundShrimp.shrimpUniqueId : null;
    }

    /// <summary>
    /// Points this display at a shrimp and fills in everything at once, no animation.
    /// Call on battle start and whenever a different shrimp switches in.
    /// </summary>
    public void Bind(UIShrimpState shrimp)
    {
        boundShrimp = shrimp;

        if (shrimp == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        SetName(shrimp.displayName);
        SetPortrait(shrimp.pfpId);
        SetSpeed(shrimp.speed);
        SetAttack(shrimp.attack);

        if (healthBar != null)
        {
            healthBar.SetValueInstant(shrimp.currentHP, shrimp.maxHP);
        }

        RefreshStatuses(shrimp.statusEffects);
    }

    /// <summary>
    /// Unbinds and hides the panel. Not the same as a KO -- a fainted shrimp keeps its display
    /// until something switches in to replace it.
    /// </summary>
    public void Clear()
    {
        boundShrimp = null;

        if (statusIcons != null)
        {
            statusIcons.ClearAll();
        }

        SetVisible(false);
    }


    // setters =============================================================================

    public void SetName(string displayName)
    {
        if (nameText != null)
        {
            nameText.text = displayName;
        }
    }

    /// <summary>
    /// Sets the profile picture from a <c>pfpID</c>. Falls back to placeholder art if the id
    /// doesn't resolve, which is most of them right now.
    /// </summary>
    public void SetPortrait(string pfpId)
    {
        if (portrait != null)
        {
            portrait.sprite = UISprites.Get(pfpId);
        }
    }

    /// <summary>
    /// Moves the hp bar to a new value. <paramref name="current"/> should be the event's
    /// <c>finalValue</c> so we can't drift out of sync with the logic.
    /// </summary>
    public void SetHealth(int current, int max, Action onComplete = null)
    {
        if (boundShrimp != null)
        {
            boundShrimp.currentHP = current;
        }

        if (healthBar == null)
        {
            onComplete?.Invoke();
            return;
        }

        healthBar.SetValue(current, max, onComplete);
    }

    /// <summary>
    /// Updates the speed readout.
    ///
    /// Note that nothing currently tells the UI when a status changes a shrimp's resolved
    /// speed, so in practice this only gets called on bind. See the TODO in StatusHandler.
    /// </summary>
    public void SetSpeed(int speed, Action onComplete = null)
    {
        if (boundShrimp != null)
        {
            boundShrimp.speed = speed;
        }

        if (speedText != null)
        {
            // TODO (animation): count the number up/down instead of snapping
            speedText.text = speed.ToString();
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// Updates the attack readout. Same caveat as <c>SetSpeed</c>.
    /// </summary>
    public void SetAttack(int attack, Action onComplete = null)
    {
        if (boundShrimp != null)
        {
            boundShrimp.attack = attack;
        }

        if (attackText != null)
        {
            // TODO (animation): count the number up/down instead of snapping
            attackText.text = attack.ToString();
        }

        onComplete?.Invoke();
    }


    // statuses ============================================================================

    /// <summary>
    /// Rebuilds the whole status row. Use on bind / switch in.
    /// </summary>
    public void RefreshStatuses(List<AppliedStatus> statuses, Action onComplete = null)
    {
        if (statusIcons == null)
        {
            onComplete?.Invoke();
            return;
        }

        statusIcons.Refresh(statuses, onComplete);
    }

    /// <summary>
    /// Adds one icon for a status that was just applied, rather than rebuilding the row.
    /// Preferred over <c>RefreshStatuses</c> when reacting to a single event.
    /// </summary>
    public void AddStatus(AppliedStatus applied, Action onComplete = null)
    {
        if (statusIcons == null)
        {
            onComplete?.Invoke();
            return;
        }

        statusIcons.AddStatus(applied, onComplete);
    }

    /// <summary>
    /// Drops the icon for a status that just expired. Does nothing if no icon matches.
    /// </summary>
    public void RemoveStatus(string statusId, Action onComplete = null)
    {
        if (statusIcons == null)
        {
            onComplete?.Invoke();
            return;
        }

        statusIcons.RemoveStatus(statusId, onComplete);
    }


    // reactions ===========================================================================

    /// <summary>
    /// The little "ow" wobble when this shrimp gets hit.
    /// </summary>
    public void PlayHurtReaction(Action onComplete = null)
    {
        // TODO (animation): shake reactionRoot / flash the portrait red, then invoke onComplete
        onComplete?.Invoke();
    }

    /// <summary>
    /// The counterpart to <c>PlayHurtReaction</c> for healing.
    /// </summary>
    public void PlayHealReaction(Action onComplete = null)
    {
        // TODO (animation): bob reactionRoot / flash the portrait green, then invoke onComplete
        onComplete?.Invoke();
    }

    /// <summary>
    /// Plays when this shrimp goes down. The display stays bound afterwards so the
    /// switch in can replace it.
    /// </summary>
    public void PlayDeathReaction(Action onComplete = null)
    {
        // TODO (animation): fade/drop the whole display, then invoke onComplete
        onComplete?.Invoke();
    }

    /// <summary>
    /// Plays when a new shrimp is sent out. Call after <c>Bind()</c>.
    /// </summary>
    public void PlaySwitchInReaction(Action onComplete = null)
    {
        // TODO (animation): slide the display back in, then invoke onComplete
        onComplete?.Invoke();
    }

    /// <summary>
    /// Generic "something happened to this shrimp" nudge, used for abilities and statuses
    /// that don't have their own reaction yet.
    /// </summary>
    public void PlayGenericReaction(Action onComplete = null)
    {
        // TODO (animation): small pulse on reactionRoot, then invoke onComplete
        onComplete?.Invoke();
    }

    // fades the whole panel rather than deactivating it, so layout doesn't jump around
    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
    }
}
