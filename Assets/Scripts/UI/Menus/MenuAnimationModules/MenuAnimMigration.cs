using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// written by Claude Opus 5
// helpers for the "Migrate animation to modules" buttons on the three menus.
//
// those buttons exist because the menu animations used to be hardcoded DOTween sequences, and
// converting them by hand would have meant re-dragging every target and re-typing every timing.
// instead each menu reads its own existing serialized refs and builds the modules itself.
//
// once all three menus are migrated and you're happy with them, this file and the legacy
// target fields on the menus can go.
public static class MenuAnimMigration
{
    /// <summary>
    /// Removes any transition modules already on this object, so re-running a migration
    /// replaces them instead of stacking duplicates.
    /// </summary>
    public static void ClearExisting(GameObject host)
    {
        MenuAnim_Transition[] existing = host.GetComponents<MenuAnim_Transition>();

        foreach (MenuAnim_Transition module in existing)
        {
#if UNITY_EDITOR
            // has to be DestroyImmediate -- Destroy is a no-op outside play mode
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(module);
                continue;
            }
#endif
            Object.Destroy(module);
        }
    }

    /// <summary>
    /// Adds a configured transition module to <paramref name="host"/>.
    /// </summary>
    public static MenuAnim_Transition Add(GameObject host, string label,
        IEnumerable<Component> targets, MenuTransitionSettings opening,
        MenuTransitionSettings closing)
    {
        MenuAnim_Transition module = host.AddComponent<MenuAnim_Transition>();
        module.Configure(label, targets, opening, closing);
        return module;
    }

    // ---- settings builders -------------------------------------------------------------
    // positional on purpose, so a migration method reads as a table of the original values

    public static MenuTransitionSettings Settings(float delay, float stagger,
        MenuMoveChannel move = null, MenuFloatChannel fade = null, MenuFloatChannel scale = null)
    {
        return new MenuTransitionSettings
        {
            delay = delay,
            stagger = stagger,
            move = move ?? new MenuMoveChannel(),
            fade = fade ?? new MenuFloatChannel(),
            scale = scale ?? new MenuFloatChannel()
        };
    }

    public static MenuMoveChannel Move(MenuMoveAxis axis, float value, float duration, Ease ease,
        MenuMoveSpace space = MenuMoveSpace.Anchored)
    {
        return new MenuMoveChannel
        {
            enabled = true,
            axis = axis,
            space = space,
            value = value,
            duration = duration,
            ease = ease
        };
    }

    public static MenuFloatChannel Channel(float value, float duration, Ease ease)
    {
        return new MenuFloatChannel
        {
            enabled = true,
            value = value,
            duration = duration,
            ease = ease
        };
    }

    /// <summary>
    /// Makes an edit made from a context menu actually stick to the scene. No-op in a build.
    /// </summary>
    public static void MarkDirty(Object target)
    {
#if UNITY_EDITOR
        if (target == null)
            return;

        UnityEditor.EditorUtility.SetDirty(target);

        if (!Application.isPlaying && target is Component component)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                component.gameObject.scene);
        }
#endif
    }
}
