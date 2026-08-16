using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// written by Claude Opus 5
// runs the rebinding while the settings screen is open.
//
// the whole design turns on one idea: EDITS DON'T GO LIVE UNTIL YOU LEAVE THE SCREEN.
// while settings is open the game runs on factory default controls, and the player's changes
// pile up in `pending` instead. that's what stops someone binding every action to "A" and
// then being unable to navigate the very screen they'd fix it on.
//
// so a rebind goes: capture the key -> immediately undo it on the live action -> remember it
// in pending -> show the pending value on the row. nothing the player presses in here changes
// how the menu itself responds.
public class KeybindEditSession : MonoBehaviour
{
    [Header("behaviour")]
    [SerializeField, Tooltip("wipe every keybind back to factory if the player leaves with " +
        "two actions on the same key")]
    private bool resetAllOnConflict = true;

    // what each action SHOULD be bound to once the player leaves. keyed by action, values are
    // input system control paths like "<Keyboard>/q"
    private Dictionary<KeybindAction, string> pending = new();

    // actions currently sharing a key with another action
    private HashSet<KeybindAction> conflicts = new();

    private InputActionRebindingExtensions.RebindingOperation activeOperation;
    private KeybindAction activeKeybind;
    private Coroutine rebindRoutine;

    /// <summary>
    /// Fires whenever the conflict set changes, so rows can restyle themselves.
    /// </summary>
    public event Action onConflictsChanged;

    /// <summary>
    /// Fires when a pending binding changes, so rows can refresh their text.
    /// </summary>
    public event Action<KeybindAction> onBindingChanged;

    public bool IsSessionOpen { get; private set; }
    public bool IsRebinding => activeOperation != null;
    public bool HasConflicts => conflicts.Count > 0;


    // opening and closing  =================================================================

    /// <summary>
    /// Call when the settings screen opens.
    ///
    /// Snapshots the player's current binds into <c>pending</c> FIRST (while they're still
    /// live and readable), then drops the running game to factory defaults.
    /// </summary>
    public void Begin()
    {
        if (IsSessionOpen)
            return;

        pending.Clear();

        foreach (KeybindAction keybind in KeybindRegistry.All)
        {
            KeybindTarget target = KeybindRegistry.GetDisplayTarget(keybind);
            pending[keybind] = target.EffectivePath;
        }

        // everything from here until End() runs on default controls
        GameInput.SuspendOverrides();

        IsSessionOpen = true;

        // the rows subscribed and drew themselves back when the menu object was enabled,
        // which happens before this runs -- so at that point pending was still empty and they
        // all rendered "?". poke them now that there's something to show.
        foreach (KeybindAction keybind in KeybindRegistry.All)
        {
            onBindingChanged?.Invoke(keybind);
        }

        RefreshConflicts();
    }

    /// <summary>
    /// Call when the settings screen closes. Commits the player's edits, or factory resets
    /// everything if they left two actions fighting over the same key.
    /// </summary>
    public void End()
    {
        if (!IsSessionOpen)
            return;

        CancelActiveRebind();
        IsSessionOpen = false;

        if (HasConflicts && resetAllOnConflict)
        {
            // TODO: this is the "your keybinds were reset" moment. if you want a confirmation
            // popup or a toast on the way out, it goes here.
            Debug.LogWarning("[KeybindEditSession] >> left the settings screen with duplicate " +
                "keybinds, so everything got reset to factory defaults.");

            GameInput.ResetToDefaults();
            pending.Clear();
            conflicts.Clear();
            return;
        }

        ApplyPendingToLiveControls();

        // hand the freshly applied state back as "the player's binds" and come out of
        // suspension holding it
        GameInput.ReplaceSuspendedOverrides(GameInput.Controls.SaveBindingOverridesAsJson());
        GameInput.ResumeOverrides();
        GameInput.SaveOverrides();
    }

    /// <summary>
    /// Throws away the player's edits and puts every row back to factory defaults.
    /// Wire this to a "reset to defaults" button if you add one.
    /// </summary>
    public void ResetPendingToDefaults()
    {
        CancelActiveRebind();

        foreach (KeybindAction keybind in KeybindRegistry.All)
        {
            // we're suspended, so what's live IS the default
            KeybindTarget target = KeybindRegistry.GetDisplayTarget(keybind);
            pending[keybind] = target.EffectivePath;

            onBindingChanged?.Invoke(keybind);
        }

        RefreshConflicts();
    }


    // rebinding  ===========================================================================

    /// <summary>
    /// Starts listening for the player's next key and assigns it to <paramref name="keybind"/>.
    /// </summary>
    public void StartRebind(KeybindAction keybind)
    {
        if (!IsSessionOpen)
        {
            Debug.LogWarning("[KeybindEditSession] >> StartRebind before Begin(). " +
                "is SettingsMenu calling Begin() when it opens?");
            return;
        }

        CancelActiveRebind();

        activeKeybind = keybind;
        rebindRoutine = StartCoroutine(RebindRoutine(keybind));
    }

    // the one frame wait matters: this gets called from the submit handler while the confirm
    // key is still physically down, and starting the operation in the same frame can catch
    // that same press as the "new" binding
    IEnumerator RebindRoutine(KeybindAction keybind)
    {
        yield return null;

        KeybindTarget target = KeybindRegistry.GetDisplayTarget(keybind);

        if (target.action == null)
        {
            Debug.LogWarning($"[KeybindEditSession] >> nothing to rebind for {keybind}.");
            rebindRoutine = null;
            yield break;
        }

        activeOperation = target.action.PerformInteractiveRebinding(target.bindingIndex)
            .WithControlsExcluding("<Mouse>")
            .WithControlsExcluding("<Pen>")
            // no cancel key on purpose. escape has to stay bindable (it's the default Pause
            // key), and since literally any key completes the rebind the player can always
            // get out of the waiting state anyway.
            .WithCancelingThrough("")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => FinishRebind(keybind, target, operation))
            .OnCancel(operation => FinishRebind(keybind, target, operation));

        activeOperation.Start();
        rebindRoutine = null;
    }

    // THE IMPORTANT BIT: the rebinding operation has just written an override onto the live
    // action. we read it, then immediately strip it back off, so the settings screen keeps
    // running on defaults and the change only exists in `pending`.
    private void FinishRebind(KeybindAction keybind, KeybindTarget target,
        InputActionRebindingExtensions.RebindingOperation operation)
    {
        string capturedPath = null;

        if (target.action != null && target.bindingIndex < target.action.bindings.Count)
        {
            capturedPath = target.action.bindings[target.bindingIndex].overridePath;
            target.action.RemoveBindingOverride(target.bindingIndex);
        }

        operation.Dispose();
        activeOperation = null;

        // a cancelled operation leaves no override, so keep whatever was there before
        if (!string.IsNullOrEmpty(capturedPath))
        {
            pending[keybind] = capturedPath;
        }

        onBindingChanged?.Invoke(keybind);
        RefreshConflicts();
    }

    /// <summary>
    /// Aborts a rebind that's mid-flight. Safe to call when nothing is running.
    /// </summary>
    public void CancelActiveRebind()
    {
        if (rebindRoutine != null)
        {
            StopCoroutine(rebindRoutine);
            rebindRoutine = null;
        }

        if (activeOperation == null)
            return;

        // Cancel() runs our OnCancel, which disposes and clears activeOperation
        activeOperation.Cancel();

        if (activeOperation != null)
        {
            activeOperation.Dispose();
            activeOperation = null;
        }
    }


    // reading state  =======================================================================

    /// <summary>
    /// The control path this action WILL have once the player leaves the screen.
    /// This is what rows should display -- not the live binding, which is deliberately
    /// stuck on defaults while settings is open.
    /// </summary>
    public string GetPendingPath(KeybindAction keybind)
    {
        return pending.TryGetValue(keybind, out string path) ? path : null;
    }

    /// <summary>
    /// A human readable version of <c>GetPendingPath</c>, e.g. "Q" or "Left Arrow".
    /// </summary>
    public string GetPendingDisplayString(KeybindAction keybind)
    {
        string path = GetPendingPath(keybind);

        if (string.IsNullOrEmpty(path))
            return "?";

        return InputControlPath.ToHumanReadableString(
            path, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    public bool IsConflicting(KeybindAction keybind)
    {
        return conflicts.Contains(keybind);
    }

    /// <summary>
    /// Every action currently sharing its key with another action.
    /// </summary>
    public IReadOnlyCollection<KeybindAction> GetConflicts()
    {
        return conflicts;
    }


    // internals  ===========================================================================

    // rebuilds the conflict set and tells anyone listening
    private void RefreshConflicts()
    {
        int previousCount = conflicts.Count;
        conflicts.Clear();

        foreach (KeybindAction a in KeybindRegistry.All)
        {
            string pathA = GetPendingPath(a);

            if (string.IsNullOrEmpty(pathA))
                continue;

            foreach (KeybindAction b in KeybindRegistry.All)
            {
                if (a == b)
                    continue;

                if (pathA == GetPendingPath(b))
                {
                    conflicts.Add(a);
                    conflicts.Add(b);
                }
            }
        }

        // TODO: this is where the "duplicate keybinds, these will be reset if you leave"
        // warning text should be shown/hidden. KeybindRow.SetConflicting handles the
        // per-row highlight; the screen wide warning has no home yet because you hadn't
        // decided what it should say.
        if (conflicts.Count != previousCount || conflicts.Count > 0)
        {
            onConflictsChanged?.Invoke();
        }
    }

    // writes every pending path onto the real controls as an override.
    // for the navigation actions this hits BOTH slots (see KeybindRegistry.GetTargets)
    private void ApplyPendingToLiveControls()
    {
        foreach (KeybindAction keybind in KeybindRegistry.All)
        {
            string path = GetPendingPath(keybind);

            if (string.IsNullOrEmpty(path))
                continue;

            foreach (KeybindTarget target in KeybindRegistry.GetTargets(keybind))
            {
                // an override matching the authored default is just noise in the saved json
                if (target.action.bindings[target.bindingIndex].path == path)
                {
                    target.action.RemoveBindingOverride(target.bindingIndex);
                }
                else
                {
                    target.action.ApplyBindingOverride(target.bindingIndex, path);
                }
            }
        }
    }

    private void OnDisable()
    {
        CancelActiveRebind();
    }
}
