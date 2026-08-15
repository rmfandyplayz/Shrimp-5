using Sh.UIContract;
using System.Collections;
using UnityEngine;

// written by Claude Opus 5
// handles shrimp coming and going -- someone getting knocked out, and someone else stepping in.
//
// note that SwitchingShrimp is a RESULT, not a request. by the time we see it the swap has
// already happened in the logic and sourceId is whoever is now out. the player asking to
// switch is handled entirely on the ui side (see BattleUIInput).
public class RosterHandler : BattleEventHandlerBase
{
    [Header("minion refs")]
    [SerializeField] private CommandBox commandBox;
    [SerializeField] private InfoDisplayGroup infoDisplays;

    [Header("timing")]
    [SerializeField, Tooltip("beat after a line is read before the next event starts")]
    private float postLineDelay = 0.35f;

    protected override BattleEventType[] HandledTypes => new[]
    {
        BattleEventType.SwitchingShrimp,
        BattleEventType.CharacterDied
    };

    protected override IEnumerator Handle(BattleEvent evt)
    {
        switch (evt.eventType)
        {
            case BattleEventType.SwitchingShrimp:
                yield return HandleSwitchingShrimp(evt);
                break;

            case BattleEventType.CharacterDied:
                yield return HandleCharacterDied(evt);
                break;
        }
    }

    /// <summary>
    /// Repoints a display at whoever just stepped out.
    ///
    /// This is the one place a full <c>Bind()</c> is right instead of individual setters:
    /// everything on the panel changes at once, and it's the only moment the live status list
    /// can be trusted, since the incoming shrimp's statuses are simply whatever they are now.
    /// </summary>
    IEnumerator HandleSwitchingShrimp(BattleEvent evt)
    {
        UIShrimpState incoming = GetShrimp(evt.sourceId);
        ShrimpInfoDisplay display = GetDisplay(evt.sourceId);

        yield return Say(BattleTextBuilder.SwitchingShrimp(evt, incoming));

        if (display != null)
        {
            display.Bind(incoming);

            yield return WaitFor(done => display.PlaySwitchInReaction(done));
        }

        yield return WaitOrSkip(postLineDelay);
    }

    /// <summary>
    /// Plays a KO. The display stays bound afterwards -- a SwitchingShrimp event is what
    /// replaces it, and there may be a gap between the two.
    /// </summary>
    IEnumerator HandleCharacterDied(BattleEvent evt)
    {
        UIShrimpState fallen = GetShrimp(evt.sourceId);
        ShrimpInfoDisplay display = GetDisplay(evt.sourceId);

        yield return Say(BattleTextBuilder.CharacterDied(evt, fallen));

        if (display != null)
        {
            // make sure the bar reads empty even if we somehow missed the killing blow
            yield return WaitFor(done => display.SetHealth(0, GetMaxHP(fallen), done));
            yield return WaitFor(done => display.PlayDeathReaction(done));
        }

        yield return WaitOrSkip(postLineDelay);
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

    private static int GetMaxHP(UIShrimpState shrimp)
    {
        return shrimp != null ? shrimp.maxHP : 1;
    }

    protected override void OnForceSkip()
    {
        if (commandBox != null)
        {
            commandBox.SkipDialogue();
        }
    }
}
