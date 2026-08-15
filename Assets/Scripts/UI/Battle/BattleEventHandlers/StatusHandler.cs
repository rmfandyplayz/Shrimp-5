using Sh.UIContract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// written by Claude Opus 5
// handles statuses going on and coming off, plus passive abilities firing.
//
// the status id arrives in strings[0] rather than on a dedicated field. heads up that
// statusID is currently blank on every status asset, so that string is usually empty --
// the code below falls back to matching by list position so it still does something sensible.
public class StatusHandler : BattleEventHandlerBase
{
    [Header("minion refs")]
    [SerializeField] private CommandBox commandBox;
    [SerializeField] private InfoDisplayGroup infoDisplays;

    [Header("timing")]
    [SerializeField, Tooltip("beat after a line is read before the next event starts")]
    private float postLineDelay = 0.35f;

    protected override BattleEventType[] HandledTypes => new[]
    {
        BattleEventType.StatusApplied,
        BattleEventType.StatusRemoved,
        BattleEventType.AbilityTriggered
    };

    protected override IEnumerator Handle(BattleEvent evt)
    {
        switch (evt.eventType)
        {
            case BattleEventType.StatusApplied:
                yield return HandleStatusApplied(evt);
                break;

            case BattleEventType.StatusRemoved:
                yield return HandleStatusRemoved(evt);
                break;

            case BattleEventType.AbilityTriggered:
                yield return HandleAbilityTriggered(evt);
                break;
        }
    }

    /// <summary>
    /// Adds an icon for a newly applied status.
    ///
    /// Falls back to resyncing the whole status row when the id can't be matched, which is
    /// common since <c>statusID</c> is blank on the assets.
    /// </summary>
    IEnumerator HandleStatusApplied(BattleEvent evt)
    {
        UIShrimpState shrimp = GetShrimp(evt.sourceId);
        ShrimpInfoDisplay display = GetDisplay(evt.sourceId);

        string statusId = GetFirstString(evt);
        AppliedStatus applied = FindAppliedStatus(shrimp, statusId);
        StatusDefinition definition = applied != null ? applied.status : null;

        yield return Say(BattleTextBuilder.StatusApplied(evt, shrimp, definition));

        if (display != null)
        {
            yield return WaitFor(done => display.PlayGenericReaction(done));

            if (applied != null)
            {
                yield return WaitFor(done => display.AddStatus(applied, done));
            }
            else
            {
                // couldn't pin down which status it was, so just resync the whole row
                yield return WaitFor(done => display.RefreshStatuses(
                    shrimp != null ? shrimp.statusEffects : null, done));
            }

            RefreshStatsAfterStatusChange(display, shrimp);
        }

        yield return WaitOrSkip(postLineDelay);
    }

    /// <summary>
    /// Drops the icon for a status that expired.
    /// </summary>
    IEnumerator HandleStatusRemoved(BattleEvent evt)
    {
        UIShrimpState shrimp = GetShrimp(evt.sourceId);
        ShrimpInfoDisplay display = GetDisplay(evt.sourceId);

        string statusId = GetFirstString(evt);

        // this usually comes back null: the logic already pulled the status off the shrimp's
        // list before we got here, so there's nothing left to look up. the text builder falls
        // back to a generic "is back to normal" line when that happens.
        // worth revisiting if owen can send the status's display name along on the event
        StatusDefinition definition = BattleTextBuilder.FindStatus(shrimp, statusId);

        yield return Say(BattleTextBuilder.StatusRemoved(evt, shrimp, definition));

        if (display != null)
        {
            if (!string.IsNullOrEmpty(statusId))
            {
                yield return WaitFor(done => display.RemoveStatus(statusId, done));
            }
            else
            {
                // no usable id -- resync off the live list, which has already dropped it
                yield return WaitFor(done => display.RefreshStatuses(
                    shrimp != null ? shrimp.statusEffects : null, done));
            }

            RefreshStatsAfterStatusChange(display, shrimp);
        }

        yield return WaitOrSkip(postLineDelay);
    }

    /// <summary>
    /// Announces a passive ability firing. Abilities have no icon row of their own, so this
    /// is text plus a nudge on the shrimp's panel.
    /// </summary>
    IEnumerator HandleAbilityTriggered(BattleEvent evt)
    {
        UIShrimpState shrimp = GetShrimp(evt.sourceId);
        ShrimpInfoDisplay display = GetDisplay(evt.sourceId);

        yield return Say(BattleTextBuilder.AbilityTriggered(evt, shrimp));

        if (display != null)
        {
            yield return WaitFor(done => display.PlayGenericReaction(done));
        }

        yield return WaitOrSkip(postLineDelay);
    }

    // TODO: statuses change what a shrimp's attack/speed resolve to, but nothing tells the ui
    // what the new numbers are. UIShrimpState.speed/attack are snapshots taken at battle start,
    // and recomputing them here would mean copy-pasting ShrimpState.GetSpeed/GetAttack into the
    // ui where it'd rot the moment owen changes the formula.
    // ask owen to either put the new value in deltaValue/finalValue on the status events, or
    // add a stat-changed event. until then the stat text just stays put.
    private void RefreshStatsAfterStatusChange(ShrimpInfoDisplay display, UIShrimpState shrimp)
    {
        if (display == null || shrimp == null)
            return;

        display.SetSpeed(shrimp.speed);
        display.SetAttack(shrimp.attack);
    }

    /// <summary>
    /// Pulls the <c>AppliedStatus</c> off the shrimp so we can show its icon and turn counter.
    ///
    /// Matches on <c>statusID</c> when there is one. When there isn't -- which is currently
    /// always -- it assumes the newly applied status is the last one on the list. That holds
    /// for StatusApplied because the logic appends, but it's a guess, and it's why an id on
    /// the event would be worth asking Owen for.
    /// </summary>
    private static AppliedStatus FindAppliedStatus(UIShrimpState shrimp, string statusId)
    {
        if (shrimp == null || shrimp.statusEffects == null || shrimp.statusEffects.Count == 0)
            return null;

        List<AppliedStatus> statuses = shrimp.statusEffects;

        if (!string.IsNullOrEmpty(statusId))
        {
            foreach (AppliedStatus applied in statuses)
            {
                if (applied != null && applied.status != null && applied.status.statusID == statusId)
                    return applied;
            }
        }

        // statusID is blank on every status asset right now, so as a fallback assume the one
        // that was just applied is the one on the end of the list
        return statuses[statuses.Count - 1];
    }

    IEnumerator Say(string line)
    {
        if (commandBox == null || string.IsNullOrEmpty(line))
            yield break;

        yield return WaitFor(done => commandBox.SetDialogue(line, done));
    }

    private ShrimpInfoDisplay GetDisplay(string shrimpId)
    {
        return infoDisplays != null ? infoDisplays.Get(shrimpId) : null;
    }

    private static string GetFirstString(BattleEvent evt)
    {
        if (evt.strings == null || evt.strings.Count == 0)
            return null;

        return evt.strings[0];
    }

    protected override void OnForceSkip()
    {
        if (commandBox != null)
        {
            commandBox.SkipDialogue();
        }
    }
}
