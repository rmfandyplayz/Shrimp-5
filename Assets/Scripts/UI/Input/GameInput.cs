using UnityEngine;
using UnityEngine.InputSystem;

// written by Claude Opus 5
// the one and only GameControls instance for the whole game.
//
// WHY THIS EXISTS: the generated GameControls wrapper builds a brand new copy of the action
// asset from embedded json every time you call `new GameControls()`. so when MenuBase and
// BattleUIInput each did that, the game was running four completely independent copies of the
// controls, and the EventSystem was running a fifth (the project asset itself). rebinding any
// one of them changed nothing anywhere else, which is exactly why the old rebind screen didn't
// work. everything reads GameInput.Controls now so there's only ever one.
//
// also owns saving/loading rebinds, and the suspend/resume pair the settings screen uses so
// the player can't softlock themselves mid-rebind.
public static class GameInput
{
    // same PlayerPrefs key the old (deleted) MenuManager rebind code used, so anyone who
    // played the jam build keeps their binds
    private const string REBINDS_PREF_KEY = "Rebinds";

    private static GameControls controls;

    // stashed overrides while the settings screen is open. null when not suspended.
    private static string suspendedOverridesJson;

    /// <summary>
    /// The shared controls. Never call <c>new GameControls()</c> anywhere else -- you'd get a
    /// private copy that rebinding can't reach.
    /// </summary>
    public static GameControls Controls
    {
        get
        {
            if (controls == null)
            {
                controls = new GameControls();

                // the battle map stays on for the whole game. it used to get enabled/disabled
                // per menu in OnEnable/OnDisable, which was survivable when everyone had their
                // own copy but would now let the last menu to close kill input for everything.
                // menus already gate themselves on cg.interactable, so nothing is lost.
                controls.Battle.Enable();

                LoadOverrides();
            }

            return controls;
        }
    }

    /// <summary>
    /// Whether overrides are currently parked (i.e. the settings screen is open and the game is
    /// deliberately running on factory defaults).
    /// </summary>
    public static bool IsSuspended => suspendedOverridesJson != null;


    // persistence  =========================================================================

    /// <summary>
    /// Writes the current rebinds to PlayerPrefs.
    /// </summary>
    public static void SaveOverrides()
    {
        if (controls == null)
            return;

        string json = controls.SaveBindingOverridesAsJson();

        PlayerPrefs.SetString(REBINDS_PREF_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Restores rebinds from PlayerPrefs. Called automatically the first time
    /// <c>Controls</c> is touched, so you rarely need this directly.
    /// </summary>
    public static void LoadOverrides()
    {
        if (!PlayerPrefs.HasKey(REBINDS_PREF_KEY))
            return;

        string json = PlayerPrefs.GetString(REBINDS_PREF_KEY);

        if (string.IsNullOrEmpty(json))
            return;

        Controls.LoadBindingOverridesFromJson(json);
    }

    /// <summary>
    /// Wipes every rebind back to what's authored in the .inputactions asset, and forgets the
    /// saved ones too. This is what runs when the player leaves the settings screen with
    /// conflicting binds.
    /// </summary>
    public static void ResetToDefaults()
    {
        Controls.RemoveAllBindingOverrides();

        suspendedOverridesJson = null;

        PlayerPrefs.DeleteKey(REBINDS_PREF_KEY);
        PlayerPrefs.Save();
    }


    // suspend / resume  ====================================================================

    /// <summary>
    /// Parks the player's rebinds and drops the live controls to factory defaults.
    ///
    /// The settings screen calls this on open. The point is that whatever mess the player has
    /// made of their binds, the settings menu itself is always navigable with the default
    /// arrows/Z/X -- so they can't bind everything to one key and lock themselves out of the
    /// screen where they'd fix it.
    /// </summary>
    public static void SuspendOverrides()
    {
        if (IsSuspended)
            return;

        suspendedOverridesJson = Controls.SaveBindingOverridesAsJson();
        Controls.RemoveAllBindingOverrides();
    }

    /// <summary>
    /// Puts the player's rebinds back after <c>SuspendOverrides</c>.
    /// Safe to call when nothing is suspended.
    /// </summary>
    public static void ResumeOverrides()
    {
        if (!IsSuspended)
            return;

        string json = suspendedOverridesJson;
        suspendedOverridesJson = null;

        Controls.LoadBindingOverridesFromJson(json);
    }

    /// <summary>
    /// Replaces the parked rebinds with a new set, so that when the settings screen closes and
    /// calls <c>ResumeOverrides</c> the player's edits are what comes back.
    /// </summary>
    public static void ReplaceSuspendedOverrides(string overridesJson)
    {
        if (!IsSuspended)
        {
            Debug.LogWarning("[GameInput] >> ReplaceSuspendedOverrides called while nothing " +
                "was suspended. did the settings screen forget to call SuspendOverrides?");
            return;
        }

        suspendedOverridesJson = overridesJson;
    }
}
