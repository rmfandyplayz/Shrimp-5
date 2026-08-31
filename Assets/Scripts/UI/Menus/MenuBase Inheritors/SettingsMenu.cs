using UnityEngine;

// written by andy
// animation moved out into MenuAnim_Transition modules, keybind session wired, by Claude Opus 5
// the settings screen.
//
// the animation is in the MenuAnim_Transition components on this object. what's left in here is
// key rebinding: opening this screen drops the game to factory default controls and closing it
// commits whatever the player changed. see KeybindEditSession for why it works that way.
public class SettingsMenu : MenuBase
{
    [Header("keybinding")]
    [SerializeField, Tooltip("leave blank to find one on this object or its children")]
    private KeybindEditSession keybindSession;

    protected override void Awake()
    {
        base.Awake();

        if (keybindSession == null)
        {
            keybindSession = GetComponentInChildren<KeybindEditSession>(includeInactive: true);
        }

        if (keybindSession == null)
        {
            Debug.LogWarning($"[SettingsMenu] >> no KeybindEditSession found, so the key " +
                $"rebinding rows won't do anything. add one to this object.");
        }
    }

    /// <summary>
    /// Starts the keybind editing session. From here until the menu closes the game runs on
    /// factory default controls, so a player who has bound everything to one key can still
    /// navigate the screen that lets them fix it.
    /// </summary>
    public override void OnMenuOpened()
    {
        base.OnMenuOpened();

        keybindSession?.Begin();
    }

    /// <summary>
    /// Commits the player's rebinds, or factory resets them if they left with duplicates.
    /// </summary>
    public override void OnMenuClosed()
    {
        base.OnMenuClosed();

        keybindSession?.End();
    }
}
