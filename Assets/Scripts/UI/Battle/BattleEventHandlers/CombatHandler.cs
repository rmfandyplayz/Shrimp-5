using Sh.UIContract;
using System.Collections;
using UnityEngine;

// written by Claude Opus 5
// handles the hitting-things events: a move going off, damage landing, and healing.
//
// sequencing is the whole point of this one -- the line of text goes up first, THEN the hp
// bar moves, so the player reads what happened before they see the number change.
public class CombatHandler : BattleEventHandlerBase
{
    [Header("minion refs")]
    [SerializeField] private CommandBox commandBox;
    [SerializeField] private InfoDisplayGroup infoDisplays;

    [Header("timing")]
    [SerializeField, Tooltip("beat after a line is read before the next event starts")]
    private float postLineDelay = 0.35f;

    protected override BattleEventType[] HandledTypes => new[]
    {
        BattleEventType.Attack,
        BattleEventType.TakeDamage,
        BattleEventType.Heal
    };

    protected override IEnumerator Handle(BattleEvent evt)
    {
        switch (evt.eventType)
        {
            case BattleEventType.Attack:
                yield return HandleAttack(evt);
                break;

            case BattleEventType.TakeDamage:
                yield return HandleTakeDamage(evt);
                break;

            case BattleEventType.Heal:
                yield return HandleHeal(evt);
                break;
        }
    }

    /// <summary>
    /// "X used Y!" -- announces the move only. No numbers change here; the TakeDamage or Heal
    /// event that follows is what actually moves the bars.
    /// </summary>
    IEnumerator HandleAttack(BattleEvent evt)
    {
        UIShrimpState attacker = GetShrimp(evt.sourceId);

        yield return Say(BattleTextBuilder.Attack(evt, attacker));
        yield return WaitOrSkip(postLineDelay);
    }

    /// <summary>
    /// Says the line, plays the flinch, then drains the bar -- in that order, so the player
    /// reads what happened before the number moves.
    /// </summary>
    IEnumerator HandleTakeDamage(BattleEvent evt)
    {
        UIShrimpState victim = GetShrimp(evt.sourceId);
        ShrimpInfoDisplay display = GetDisplay(evt.sourceId);

        yield return Say(BattleTextBuilder.TakeDamage(evt, victim));

        if (display != null)
        {
            yield return WaitFor(done => display.PlayHurtReaction(done));

            // finalValue is the truth. deltaValue is only for the text, so the bar can't
            // drift out of sync with the logic even if we miss an event
            yield return WaitFor(done => display.SetHealth(evt.finalValue, GetMaxHP(evt, victim), done));
        }

        yield return WaitOrSkip(postLineDelay);
    }

    /// <summary>
    /// Mirror of <c>HandleTakeDamage</c> for healing.
    /// </summary>
    IEnumerator HandleHeal(BattleEvent evt)
    {
        UIShrimpState healed = GetShrimp(evt.sourceId);
        ShrimpInfoDisplay display = GetDisplay(evt.sourceId);

        yield return Say(BattleTextBuilder.Heal(evt, healed));

        if (display != null)
        {
            yield return WaitFor(done => display.PlayHealReaction(done));
            yield return WaitFor(done => display.SetHealth(evt.finalValue, GetMaxHP(evt, healed), done));
        }

        yield return WaitOrSkip(postLineDelay);
    }

    /// <summary>
    /// Types a line out and waits for the player to have read it.
    /// Skips silently if there's nothing to say or no command box wired up.
    /// </summary>
    IEnumerator Say(string line)
    {
        if (commandBox == null || string.IsNullOrEmpty(line))
            yield break;

        yield return WaitFor(done => commandBox.SetDialogue(line, done));
    }

    // null when the id doesn't resolve to a display, which happens while instanceIDs are unset
    private ShrimpInfoDisplay GetDisplay(string shrimpId)
    {
        return infoDisplays != null ? infoDisplays.Get(shrimpId) : null;
    }

    // logic never fills maxValue in today, so fall back to what we cached at battle start
    private static int GetMaxHP(BattleEvent evt, UIShrimpState shrimp)
    {
        if (evt.maxValue > 0)
            return evt.maxValue;

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
