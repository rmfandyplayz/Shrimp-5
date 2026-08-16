using DG.Tweening;
using UnityEngine;

// written by Claude Opus 5
// little helper for the "Restore original animation" context menus on the three menus.
//
// exists so seeding a step list reads as a table of values rather than ten lines of object
// initialiser per tween. nothing outside those context menus should need this.
public static class MenuAnimSeeding
{
    /// <summary>
    /// Builds one animation step. Positional on purpose -- the seed methods read as a table
    /// of the original hardcoded tween values.
    /// </summary>
    public static MenuAnimStep Step(string label, Component target, MenuAnimProperty property,
        float value, float delay, float duration, Ease ease)
    {
        return new MenuAnimStep
        {
            label = label,
            target = target,
            property = property,
            value = value,
            delay = delay,
            duration = duration,
            ease = ease
        };
    }

    /// <summary>
    /// Makes an edit made from a context menu actually stick to the scene.
    /// No-op in a build.
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
