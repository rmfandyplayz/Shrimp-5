using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// written by Claude Opus 5
// one row in the keybinding list. shows what an action is bound to, and starts a rebind when
// the player hits confirm on it.
//
// implements ISubmitHandler directly rather than needing a Button, because the rows in
// MainMenu.unity are plain Selectables -- this drops straight onto them as is.
//
// the visual side of "we're waiting for you to press something" and "this key is used twice"
// is deliberately unfinished, see the TODOs on SetWaitingForInput/SetConflicting.
public class KeybindRow : MonoBehaviour, ISubmitHandler, ISelectHandler, IDeselectHandler
{
    [Header("what does this row rebind?")]
    [SerializeField] private KeybindAction keybind;

    [Header("refs")]
    [SerializeField, Tooltip("the text showing the currently bound key. NOT the row's label")]
    private TextMeshProUGUI bindingText;
    [SerializeField, Tooltip("optional. the row's name. left alone if unassigned, so you can " +
        "keep the label you already typed in the scene")]
    private TextMeshProUGUI labelText;

    [Header("waiting for input")]
    [SerializeField, Tooltip("what the binding text says while listening for a key")]
    private string waitingLabel = "...";

    // found by walking up the hierarchy, so rows don't each need it dragged in
    private KeybindEditSession session;
    private bool isWaiting;

    public KeybindAction GetKeybind()
    {
        return keybind;
    }

    private void Awake()
    {
        session = GetComponentInParent<KeybindEditSession>(includeInactive: true);

        if (session == null)
        {
            Debug.LogWarning($"[KeybindRow] >> '{name}' couldn't find a KeybindEditSession " +
                $"above it. put one on the settings menu object.");
        }

        if (labelText != null)
        {
            labelText.text = KeybindRegistry.GetDisplayName(keybind);
        }
    }

    private void OnEnable()
    {
        if (session != null)
        {
            session.onBindingChanged += HandleBindingChanged;
            session.onConflictsChanged += HandleConflictsChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (session != null)
        {
            session.onBindingChanged -= HandleBindingChanged;
            session.onConflictsChanged -= HandleConflictsChanged;
        }

        SetWaitingForInput(false);
    }

    /// <summary>
    /// Re-reads the pending binding and conflict state onto the row.
    /// </summary>
    public void Refresh()
    {
        if (isWaiting)
            return; // don't clobber the "press a key" prompt

        if (bindingText != null && session != null)
        {
            bindingText.text = session.GetPendingDisplayString(keybind);
        }

        SetConflicting(session != null && session.IsConflicting(keybind));
    }

    // pressing confirm on the row starts listening for the replacement key
    void ISubmitHandler.OnSubmit(BaseEventData eventData)
    {
        if (session == null || session.IsRebinding)
            return;

        SetWaitingForInput(true);
        session.StartRebind(keybind);
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        // TODO: hook for whatever "this row is highlighted" treatment you want.
        // the row already gets Selectable's built in colour tint for free.
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        // navigating away mid rebind would leave the row stuck saying "..." forever
        if (isWaiting && session != null)
        {
            session.CancelActiveRebind();
            SetWaitingForInput(false);
            Refresh();
        }
    }

    private void HandleBindingChanged(KeybindAction changed)
    {
        if (changed != keybind)
            return;

        SetWaitingForInput(false);
        Refresh();
    }

    private void HandleConflictsChanged()
    {
        Refresh();
    }

    /// <summary>
    /// Shown while the game is listening for the player's next key.
    /// </summary>
    // TODO: decide what this should actually look like. right now it just swaps the binding
    // text for "..." because you hadn't picked between that, "Waiting for input...", a blinking
    // cursor, etc. everything needed is here -- add the animation/overlay in this one method
    // and the rest of the system doesn't care.
    public void SetWaitingForInput(bool waiting)
    {
        isWaiting = waiting;

        if (bindingText == null)
            return;

        if (waiting)
        {
            bindingText.text = waitingLabel;
        }
    }

    /// <summary>
    /// Marks the row as sharing its key with another action.
    /// </summary>
    // TODO: the highlight treatment is up to you. the detection side is done and correct --
    // KeybindEditSession works out the conflicts and pokes every row whenever they change, so
    // this method is the only place that needs to know what "conflicting" looks like.
    // (leaving the screen while conflicting factory resets the binds, which is already wired.)
    public void SetConflicting(bool conflicting)
    {
        // intentionally blank for now
    }
}
