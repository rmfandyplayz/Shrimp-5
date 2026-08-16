using System.Collections.Generic;
using UnityEngine.InputSystem;

// written by Claude Opus 5
// the list of things the player is allowed to rebind, and which actual InputAction bindings
// each one drives.
//
// the enum is what the settings rows pick in the inspector. that's deliberately NOT an
// InputActionReference like the old deleted RebindButton used -- a reference points into the
// PROJECT asset, but the game runs on GameInput's copy, so rebinding through a reference
// changed a copy nobody was reading. picking from an enum makes that mistake impossible.
public enum KeybindAction
{
    Confirm,
    Cancel,
    Secondary,
    Pause,
    NavigateUp,
    NavigateDown,
    NavigateLeft,
    NavigateRight
}

/// <summary>
/// One binding slot that a <c>KeybindAction</c> writes to.
/// Most actions have exactly one; the navigation ones have two (see the registry).
/// </summary>
public readonly struct KeybindTarget
{
    public readonly InputAction action;
    public readonly int bindingIndex;

    public KeybindTarget(InputAction action, int bindingIndex)
    {
        this.action = action;
        this.bindingIndex = bindingIndex;
    }

    /// <summary>
    /// What this slot resolves to right now -- the player's override if there is one,
    /// otherwise the authored default.
    /// </summary>
    public string EffectivePath
    {
        get
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
                return null;

            InputBinding binding = action.bindings[bindingIndex];
            return binding.effectivePath;
        }
    }
}

public static class KeybindRegistry
{
    /// <summary>
    /// Every rebindable action, in the order the settings screen lists them.
    /// </summary>
    public static readonly KeybindAction[] All =
    {
        KeybindAction.Confirm,
        KeybindAction.Cancel,
        KeybindAction.Secondary,
        KeybindAction.Pause,
        KeybindAction.NavigateUp,
        KeybindAction.NavigateDown,
        KeybindAction.NavigateLeft,
        KeybindAction.NavigateRight
    };

    /// <summary>
    /// The label shown on the row. Note these don't all match the underlying action names --
    /// the row the player reads as "Cancel" is the action called <c>Back</c>.
    /// </summary>
    public static string GetDisplayName(KeybindAction keybind)
    {
        switch (keybind)
        {
            case KeybindAction.Confirm: return "Confirm";
            case KeybindAction.Cancel: return "Cancel";
            case KeybindAction.Secondary: return "Secondary";
            case KeybindAction.Pause: return "Pause";
            case KeybindAction.NavigateUp: return "Navigate Up";
            case KeybindAction.NavigateDown: return "Navigate Down";
            case KeybindAction.NavigateLeft: return "Navigate Left";
            case KeybindAction.NavigateRight: return "Navigate Right";
            default: return keybind.ToString();
        }
    }

    /// <summary>
    /// Every binding slot a rebind of this action has to write to.
    ///
    /// The four navigation actions return TWO slots each, and that pairing is load bearing:
    /// the arrow keys are bound twice in the asset, once on <c>NavUp</c>/<c>NavDown</c>/etc
    /// (which the game reads) and once as a part of the <c>UINavigation</c> 2D vector composite
    /// (which the EventSystem navigates menus with). Rebinding only one of the pair would leave
    /// menu navigation and game navigation on different keys.
    /// </summary>
    public static List<KeybindTarget> GetTargets(KeybindAction keybind)
    {
        GameControls.BattleActions battle = GameInput.Controls.Battle;
        List<KeybindTarget> targets = new();

        switch (keybind)
        {
            // Confirm is bound three times: z, enter, numpadEnter. we only ever touch slot 0
            // (the z one) so enter always keeps working as a hardcoded alternate -- the player
            // can't rebind their way out of being able to confirm anything.
            case KeybindAction.Confirm:
                targets.Add(new KeybindTarget(battle.Confirm, 0));
                break;

            case KeybindAction.Cancel:
                targets.Add(new KeybindTarget(battle.Back, 0));
                break;

            case KeybindAction.Secondary:
                targets.Add(new KeybindTarget(battle.Inspect_Secondary, 0));
                break;

            case KeybindAction.Pause:
                targets.Add(new KeybindTarget(battle.Pause, 0));
                break;

            case KeybindAction.NavigateUp:
                targets.Add(new KeybindTarget(battle.NavUp, 0));
                targets.Add(new KeybindTarget(battle.UINavigation, FindCompositePart(battle.UINavigation, "up")));
                break;

            case KeybindAction.NavigateDown:
                targets.Add(new KeybindTarget(battle.NavDown, 0));
                targets.Add(new KeybindTarget(battle.UINavigation, FindCompositePart(battle.UINavigation, "down")));
                break;

            case KeybindAction.NavigateLeft:
                targets.Add(new KeybindTarget(battle.NavLeft, 0));
                targets.Add(new KeybindTarget(battle.UINavigation, FindCompositePart(battle.UINavigation, "left")));
                break;

            case KeybindAction.NavigateRight:
                targets.Add(new KeybindTarget(battle.NavRight, 0));
                targets.Add(new KeybindTarget(battle.UINavigation, FindCompositePart(battle.UINavigation, "right")));
                break;
        }

        // drop any slot we couldn't resolve rather than letting a -1 index through
        targets.RemoveAll(t => t.action == null || t.bindingIndex < 0);

        return targets;
    }

    /// <summary>
    /// The slot whose key the settings row actually displays. For navigation that's the
    /// <c>NavUp</c>-style binding rather than the composite part; they're kept identical
    /// anyway, so either would do.
    /// </summary>
    public static KeybindTarget GetDisplayTarget(KeybindAction keybind)
    {
        List<KeybindTarget> targets = GetTargets(keybind);
        return targets.Count > 0 ? targets[0] : default;
    }

    // walks a composite's parts looking for one by name ("up"/"down"/"left"/"right").
    // done by name rather than a hardcoded index so reordering the composite in the
    // inputactions editor doesn't quietly rebind the wrong direction.
    private static int FindCompositePart(InputAction action, string partName)
    {
        if (action == null)
            return -1;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            if (binding.isPartOfComposite && binding.name == partName)
                return i;
        }

        return -1;
    }
}
