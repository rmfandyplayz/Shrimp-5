using UnityEngine;

// written by Claude Opus 5
//
// TODO: REVISIT ONCE OWEN DECIDES HOW MOVE CATEGORIES ARE ACTUALLY STORED.
// right now MoveDefinition has no category field at all -- just power, hasEffect and target.
// so the ui guesses the category from those. everything to do with that guess lives in this
// one file on purpose: when a real category enum lands on MoveDefinition, only Classify()
// needs to change and every call site keeps working.
public enum MoveCategory
{
    Attack,
    Healing,
    Effect
}

public static class MoveCategories
{
    // fallback icons used when a move's own iconID doesn't resolve, which is currently
    // basically always. these are ids passed to UISprites, so they live in Resources/Art/UI/
    private const string ATTACK_ICON_ID = "icon.category.attack";
    private const string HEALING_ICON_ID = "icon.category.healing";
    private const string EFFECT_ICON_ID = "icon.category.effect";

    /// <summary>
    /// Works out what kind of move this is so the UI knows which icon and wording to use.
    /// </summary>
    public static MoveCategory Classify(MoveDefinition move)
    {
        if (move == null)
            return MoveCategory.Effect;

        // TODO: swap this whole block for move.category when that exists
        if (move.power < 0)
            return MoveCategory.Healing;

        if (move.power > 0)
            return MoveCategory.Attack;

        return MoveCategory.Effect;
    }

    /// <summary>
    /// The icon for a move. Prefers the move's own <c>iconID</c> and falls back to a generic
    /// per category icon, since most <c>iconID</c>s in the assets are still placeholder text.
    /// </summary>
    public static Sprite GetIcon(MoveDefinition move)
    {
        if (move == null)
            return UISprites.GetPlaceholder();

        Sprite ownIcon = UISprites.GetOrNull(move.iconID);
        if (ownIcon != null)
            return ownIcon;

        return UISprites.Get(GetFallbackIconId(Classify(move)));
    }

    /// <summary>
    /// The generic icon id for a whole category, used when a move has no usable icon of its own.
    /// </summary>
    public static string GetFallbackIconId(MoveCategory category)
    {
        switch (category)
        {
            case MoveCategory.Healing:
                return HEALING_ICON_ID;

            case MoveCategory.Attack:
                return ATTACK_ICON_ID;

            default:
                return EFFECT_ICON_ID;
        }
    }

    /// <summary>
    /// How much a move heals or hurts for, as a positive number.
    /// Power is stored negative for healing, which reads badly in a details panel.
    /// </summary>
    public static int GetMagnitude(MoveDefinition move)
    {
        if (move == null)
            return 0;

        return Mathf.Abs(move.power);
    }
}
