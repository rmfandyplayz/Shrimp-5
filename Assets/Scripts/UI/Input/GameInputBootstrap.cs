using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

// written by Claude Opus 5
// wakes GameInput up and keeps the EventSystem pointed at it.
//
// self installing -- there's nothing to drag into a scene, it just runs. that's deliberate,
// because forgetting to place it in one scene would silently give you the desync bug back.
//
// the problem it solves: InputSystemUIInputModule holds InputActionReferences that point into
// the PROJECT copy of GameControls.inputactions, while GameInput.Controls is a separate runtime
// copy. so menu navigation/submit/cancel would keep running on unrebound defaults while the
// rest of the game used the player's binds. this repoints the module at the shared copy after
// every scene load.
public static class GameInputBootstrap
{
    // InputActionReference.Create spawns a ScriptableObject, so cache them instead of making
    // fresh ones on every scene load
    private static Dictionary<string, InputActionReference> referenceCache = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // touching Controls builds the shared instance and loads the player's saved rebinds
        _ = GameInput.Controls;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // sceneLoaded doesn't reliably fire for the scene that's already loading when we subscribe,
    // so catch the first one explicitly
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyToStartupScene()
    {
        RepointUIModules();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RepointUIModules();
    }

    /// <summary>
    /// Points every <c>InputSystemUIInputModule</c> in the loaded scenes at the shared controls,
    /// keeping whichever actions each scene had wired up.
    /// </summary>
    public static void RepointUIModules()
    {
        InputSystemUIInputModule[] modules =
            Object.FindObjectsByType<InputSystemUIInputModule>(FindObjectsSortMode.None);

        foreach (InputSystemUIInputModule module in modules)
        {
            Repoint(module);
        }
    }

    private static void Repoint(InputSystemUIInputModule module)
    {
        InputActionAsset sharedAsset = GameInput.Controls.asset;

        if (module.actionsAsset == sharedAsset)
            return; // already done

        // remember what this scene had wired before swapping the asset out from under it.
        // scenes differ -- MainMenu drives navigation off the Battle map with no pointer
        // actions, while the battle ui test scene uses the full UI map with mouse. resolving
        // by name preserves whatever each one chose instead of forcing one layout on both.
        string point = PathOf(module.point);
        string move = PathOf(module.move);
        string submit = PathOf(module.submit);
        string cancel = PathOf(module.cancel);
        string leftClick = PathOf(module.leftClick);
        string middleClick = PathOf(module.middleClick);
        string rightClick = PathOf(module.rightClick);
        string scrollWheel = PathOf(module.scrollWheel);
        string trackedPos = PathOf(module.trackedDevicePosition);
        string trackedRot = PathOf(module.trackedDeviceOrientation);

        // toggling the module forces it to drop its old action hookups cleanly rather than
        // leaving callbacks attached to the project asset
        bool wasEnabled = module.enabled;
        module.enabled = false;

        module.actionsAsset = sharedAsset;

        module.point = Resolve(sharedAsset, point);
        module.move = Resolve(sharedAsset, move);
        module.submit = Resolve(sharedAsset, submit);
        module.cancel = Resolve(sharedAsset, cancel);
        module.leftClick = Resolve(sharedAsset, leftClick);
        module.middleClick = Resolve(sharedAsset, middleClick);
        module.rightClick = Resolve(sharedAsset, rightClick);
        module.scrollWheel = Resolve(sharedAsset, scrollWheel);
        module.trackedDevicePosition = Resolve(sharedAsset, trackedPos);
        module.trackedDeviceOrientation = Resolve(sharedAsset, trackedRot);

        module.enabled = wasEnabled;
    }

    // "MapName/ActionName" for an assigned reference, or null if the slot was empty
    private static string PathOf(InputActionReference reference)
    {
        if (reference == null || reference.action == null)
            return null;

        InputAction action = reference.action;

        if (action.actionMap == null)
            return action.name;

        return $"{action.actionMap.name}/{action.name}";
    }

    // finds the equivalent action in the shared asset and wraps it back up as a reference
    private static InputActionReference Resolve(InputActionAsset asset, string actionPath)
    {
        if (string.IsNullOrEmpty(actionPath))
            return null;

        if (referenceCache.TryGetValue(actionPath, out InputActionReference cached) && cached != null)
            return cached;

        InputAction action = asset.FindAction(actionPath);

        if (action == null)
        {
            Debug.LogWarning($"[GameInputBootstrap] >> the EventSystem wanted '{actionPath}' but " +
                $"the shared controls don't have it. that slot will be left empty.");
            return null;
        }

        InputActionReference reference = InputActionReference.Create(action);
        referenceCache[actionPath] = reference;

        return reference;
    }
}
