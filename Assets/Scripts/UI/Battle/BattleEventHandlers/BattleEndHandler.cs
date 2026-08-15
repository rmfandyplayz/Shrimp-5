using Sh.UIContract;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// written by Claude Opus 5
// handles the battle being over.
//
// deliberately doesn't decide what happens next -- it says the line and then fires a
// UnityEvent so the scene can wire up whatever comes after (results screen, TransitionManager
// back to the overworld, etc) without this script needing to know about any of it.
public class BattleEndHandler : BattleEventHandlerBase
{
    [Header("minion refs")]
    [SerializeField] private CommandBox commandBox;

    [Header("timing")]
    [SerializeField, Tooltip("how long the result sits on screen before handing off")]
    private float resultHoldDelay = 1.5f;

    [Header("what happens after")]
    [SerializeField, Tooltip("drag whatever should run when the player wins")]
    private UnityEvent onBattleWon;
    [SerializeField, Tooltip("drag whatever should run when the player loses")]
    private UnityEvent onBattleLost;

    protected override BattleEventType[] HandledTypes => new[]
    {
        BattleEventType.BattleWon,
        BattleEventType.BattleLost
    };

    /// <summary>
    /// Says the result, holds it on screen, then fires the matching UnityEvent.
    ///
    /// Forces the box back to dialogue mode first, since the battle can end while the player
    /// is mid-selection (their last shrimp going down on the enemy's half of the turn).
    ///
    /// Fair warning for whoever wires the UnityEvents up: <c>DeathManager.Die</c> currently
    /// queues BattleWon when the PLAYER's team wipes and BattleLost when the enemy's does,
    /// which is backwards. Check which one actually fires before trusting the names.
    /// </summary>
    protected override IEnumerator Handle(BattleEvent evt)
    {
        bool won = evt.eventType == BattleEventType.BattleWon;

        string line = won
            ? BattleTextBuilder.BattleWon(evt)
            : BattleTextBuilder.BattleLost(evt);

        if (commandBox != null)
        {
            commandBox.SwitchCommandBoxDisplayMode(CommandBoxMode.DIALOGUE_DISPLAY);
            yield return WaitFor(done => commandBox.SetDialogue(line, done));
        }

        yield return WaitOrSkip(resultHoldDelay);

        if (won)
        {
            onBattleWon?.Invoke();
        }
        else
        {
            onBattleLost?.Invoke();
        }
    }

    protected override void OnForceSkip()
    {
        if (commandBox != null)
        {
            commandBox.SkipDialogue();
        }
    }
}
