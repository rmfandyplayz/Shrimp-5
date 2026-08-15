using System;
using System.Collections;
using Sh.UIContract;
using UnityEngine;

// written by Claude Opus 5
// base class for every battle event handler. does the boring bits so the actual handlers
// are just "here's my event types, here's what happens".
//
// handlers ORCHESTRATE. they read the event, work out what should happen, and tell the
// minion scripts to do it. they should never be touching an Image or a TMP field directly.
//
// to add one: subclass this, fill in HandledTypes and HandleEvent, stick it on a child object
// under BattleUIManager and add it to the handlerScripts list.
public abstract class BattleEventHandlerBase : MonoBehaviour, IBattleEventHandler
{
    [Header("shared refs")]
    [SerializeField] protected BattleUIManager uiManager;

    // set by ForceSkip when the player wants to hurry things along. cleared at the start of
    // every event so a skip doesn't leak into the next one
    protected bool isSkipping;

    /// <summary>
    /// Which event types this handler owns.
    ///
    /// These must not overlap with another handler's -- BattleUIManager takes the FIRST
    /// handler that says yes and ignores the rest.
    /// </summary>
    protected abstract BattleEventType[] HandledTypes { get; }

    protected virtual void Awake()
    {
        if (uiManager == null)
        {
            uiManager = GetComponentInParent<BattleUIManager>();
        }

        if (uiManager == null)
        {
            Debug.LogWarning($"[{GetType().Name}] >> couldn't find a BattleUIManager. " +
                $"assign it in the inspector or make this a child of one.");
        }
    }

    public bool CanHandle(BattleEventType eventType)
    {
        foreach (BattleEventType handled in HandledTypes)
        {
            if (handled == eventType)
                return true;
        }

        return false;
    }

    public void ForceSkip()
    {
        isSkipping = true;
        OnForceSkip();
    }

    // override if your handler needs to tell a minion to snap to its end state
    protected virtual void OnForceSkip() { }

    public IEnumerator HandleEvent(BattleEvent evt)
    {
        isSkipping = false;
        yield return Handle(evt);
    }

    /// <summary>
    /// Do the thing. Yield on <c>WaitFor</c> / <c>WaitOrSkip</c> to let animations play out --
    /// BattleUIManager won't start the next event until this finishes.
    /// </summary>
    protected abstract IEnumerator Handle(BattleEvent evt);


    // helpers  ============================================================================

    /// <summary>
    /// Waits for a minion's visual change to finish.
    ///
    /// The minion methods take an <c>Action onComplete</c> rather than returning something
    /// waitable, so this adapts them for use in a coroutine:
    /// <code>yield return WaitFor(done => infoDisplay.SetHealth(hp, maxHp, done));</code>
    ///
    /// Right now they all call back immediately, so this costs a frame at most. Once the
    /// DOTween pass happens they'll call back when the tween lands and this starts actually
    /// waiting -- without any change here or in the handlers.
    /// </summary>
    protected IEnumerator WaitFor(Action<Action> visualCall)
    {
        bool done = false;
        visualCall(() => done = true);

        while (!done && !isSkipping)
        {
            yield return null;
        }
    }

    /// <summary>
    /// A <c>WaitForSeconds</c> the player can cut short by pressing confirm.
    /// </summary>
    protected IEnumerator WaitOrSkip(float seconds)
    {
        float elapsed = 0f;

        while (elapsed < seconds && !isSkipping)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Looks up the UI's copy of a shrimp's state. Returns null (and warns) when the id
    /// isn't one we know about, which happens whenever instanceID is unset on the logic side.
    /// </summary>
    protected UIShrimpState GetShrimp(string shrimpId)
    {
        if (uiManager == null)
            return null;

        if (uiManager.TryGetShrimp(shrimpId, out UIShrimpState shrimp))
            return shrimp;

        if (!string.IsNullOrEmpty(shrimpId))
        {
            Debug.LogWarning($"[{GetType().Name}] >> no cached shrimp for id '{shrimpId}'. " +
                $"was InitializeBattle called, and does the id match an instanceID?");
        }

        return null;
    }
}
