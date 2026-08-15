using System;
using System.Collections.Generic;
using UnityEngine;

// written by Claude Opus 5
// the row of status icons under a shrimp's info display. pools its icons rather than
// destroying and respawning them every time something changes.
//
// heads up on timing: the game logic resolves a whole turn in one frame while the ui is still
// chewing through the first event, and UIShrimpState.statusEffects is a live reference to the
// logic's own list. so by the time we're animating "poison applied", the list may already have
// poison removed again. that's why AddStatus/RemoveStatus take a definition straight off the
// event instead of re-reading the list -- Refresh() is only for a full resync (like a switch in).
public class StatusIconList : MonoBehaviour
{
    [Header("refs")]
    [SerializeField, Tooltip("where icons get spawned. defaults to this transform. " +
        "stick a layout group on it")]
    private Transform iconParent;
    [SerializeField] private StatusIcon iconPrefab;
    [SerializeField, Tooltip("optional. hidden when the shrimp has no statuses")]
    private CanvasGroup canvasGroup;

    [Header("icons already placed in the scene")]
    [SerializeField] private List<StatusIcon> pool = new();

    private void Awake()
    {
        if (iconParent == null)
            iconParent = transform;
    }

    /// <summary>
    /// Rebuilds the whole row from a shrimp's current status list.
    /// Use on bind / switch in, not in response to a single status event.
    /// </summary>
    public void Refresh(List<AppliedStatus> statuses, Action onComplete = null)
    {
        int wanted = statuses != null ? statuses.Count : 0;

        EnsureIconCount(wanted);

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] == null)
                continue;

            if (i < wanted)
            {
                pool[i].Bind(statuses[i]);
            }
            else
            {
                pool[i].Clear();
            }
        }

        ApplyVisibility(wanted > 0);

        // TODO (animation): stagger the icons popping in, then invoke onComplete when done
        onComplete?.Invoke();
    }

    /// <summary>
    /// Adds a single icon for a status that was just applied.
    /// If the shrimp already has an icon for it, that icon just refreshes its turn counter.
    /// </summary>
    public void AddStatus(AppliedStatus applied, Action onComplete = null)
    {
        if (applied == null || applied.status == null)
        {
            onComplete?.Invoke();
            return;
        }

        StatusIcon existing = FindIcon(applied.status.statusID);

        if (existing != null)
        {
            existing.Bind(applied);
        }
        else
        {
            StatusIcon free = GetFreeIcon();

            if (free == null)
            {
                Debug.LogWarning($"[StatusIconList] >> {name} ran out of icons and has no " +
                    $"iconPrefab to make more. '{applied.status.statusID}' won't show up.");
                onComplete?.Invoke();
                return;
            }

            free.Bind(applied);
        }

        ApplyVisibility(true);

        // TODO (animation): pop the new icon in, then invoke onComplete
        onComplete?.Invoke();
    }

    /// <summary>
    /// Drops the icon for a status that just expired.
    /// </summary>
    public void RemoveStatus(string statusId, Action onComplete = null)
    {
        StatusIcon existing = FindIcon(statusId);

        if (existing != null)
        {
            // TODO (animation): fade the icon out before clearing it, then invoke onComplete
            existing.Clear();
        }

        ApplyVisibility(GetActiveCount() > 0);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Re-reads the turn counters on every visible icon without rebuilding the row.
    /// </summary>
    public void RefreshTurnCounters()
    {
        foreach (StatusIcon icon in pool)
        {
            if (icon != null && icon.gameObject.activeSelf)
                icon.RefreshTurns();
        }
    }

    /// <summary>
    /// Empties the whole row. Used when a display stops showing anyone.
    /// </summary>
    public void ClearAll()
    {
        foreach (StatusIcon icon in pool)
        {
            if (icon != null)
                icon.Clear();
        }

        ApplyVisibility(false);
    }

    // the visible icon showing this status, or null. only searches active icons so cleared
    // ones in the pool can't match
    private StatusIcon FindIcon(string statusId)
    {
        if (string.IsNullOrEmpty(statusId))
            return null;

        foreach (StatusIcon icon in pool)
        {
            if (icon != null && icon.gameObject.activeSelf && icon.Represents(statusId))
                return icon;
        }

        return null;
    }

    // first hidden icon in the pool, spawning a new one if they're all in use.
    // returns null when the pool's full and there's no prefab to grow it with
    private StatusIcon GetFreeIcon()
    {
        foreach (StatusIcon icon in pool)
        {
            if (icon != null && !icon.gameObject.activeSelf)
                return icon;
        }

        EnsureIconCount(pool.Count + 1);

        StatusIcon last = pool.Count > 0 ? pool[pool.Count - 1] : null;
        return last != null && !last.gameObject.activeSelf ? last : null;
    }

    private int GetActiveCount()
    {
        int count = 0;

        foreach (StatusIcon icon in pool)
        {
            if (icon != null && icon.gameObject.activeSelf)
                count++;
        }

        return count;
    }

    private void EnsureIconCount(int wanted)
    {
        if (wanted <= pool.Count)
            return;

        if (iconPrefab == null)
        {
            Debug.LogWarning($"[StatusIconList] >> {name} needs {wanted} icons but only has " +
                $"{pool.Count} and no iconPrefab to make more.");
            return;
        }

        while (pool.Count < wanted)
        {
            StatusIcon spawned = Instantiate(iconPrefab, iconParent);
            spawned.name = $"{iconPrefab.name} ({pool.Count + 1})";
            spawned.Clear();
            pool.Add(spawned);
        }
    }

    // hides the whole row when there's nothing on it, so an empty layout group doesn't
    // leave a gap under the shrimp's stats
    private void ApplyVisibility(bool anyStatuses)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = anyStatuses ? 1f : 0f;
    }
}
