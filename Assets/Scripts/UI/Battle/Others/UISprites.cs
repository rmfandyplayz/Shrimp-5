using Sh.UIContract;
using UnityEngine;

// written by Claude Opus 5
// thin wrapper over ResourceManager for ui art specifically.
//
// two reasons this exists instead of calling ResourceManager directly:
//   1. it sticks the "Art/UI/" prefix on for you so nobody has to remember it
//   2. it falls back to a placeholder instead of returning null. a LOT of the iconIDs in the
//      shrimp/move/status assets are still keyboard mash, so without this the ui would just be
//      a wall of invisible images
public static class UISprites
{
    // sitting in Resources/Art/UI/ already
    private const string PLACEHOLDER_ID = "test.placeholder";

    private static Sprite placeholder;

    /// <summary>
    /// Grabs a UI sprite by its id (the <c>iconID</c> / <c>pfpID</c> / <c>shrimpSpriteID</c>
    /// fields on the definition assets).
    ///
    /// Never returns null as long as the placeholder art exists, so callers can assign the
    /// result straight to an Image without null checking.
    /// </summary>
    public static Sprite Get(string spriteId)
    {
        Sprite sprite = GetOrNull(spriteId);

        if (sprite == null)
        {
            return GetPlaceholder();
        }

        return sprite;
    }

    /// <summary>
    /// Same as <c>Get()</c> but returns null instead of the placeholder when the id doesn't
    /// resolve. Use this when "no icon" should mean an empty slot rather than a broken one,
    /// or when you want to try a fallback id of your own first.
    /// </summary>
    public static Sprite GetOrNull(string spriteId)
    {
        if (string.IsNullOrEmpty(spriteId))
            return null;

        // ResourceManager remembers misses, so a bad id only logs once no matter how often
        // this gets called
        return ResourceManager.Get<Sprite>(BattleKeys.RootPaths.UI + spriteId);
    }

    /// <summary>
    /// The stand in art shown whenever a real sprite can't be found.
    /// </summary>
    public static Sprite GetPlaceholder()
    {
        if (placeholder == null)
        {
            placeholder = ResourceManager.Get<Sprite>(BattleKeys.RootPaths.UI + PLACEHOLDER_ID);
        }

        return placeholder;
    }

    /// <summary>
    /// Whether an id actually resolves to real art, without loading or logging anything new.
    /// Handy for deciding between a specific icon and a generic one.
    /// </summary>
    public static bool Exists(string spriteId)
    {
        if (string.IsNullOrEmpty(spriteId))
            return false;

        return !ResourceManager.IsKnownMiss(BattleKeys.RootPaths.UI + spriteId);
    }
}
