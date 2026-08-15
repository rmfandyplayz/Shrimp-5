using Sh.UIContract;

// written by Claude Opus 5
// builds every line of battle text the command box says.
//
// the game logic doesn't write flavor text -- flavorText on BattleEvent is an escape hatch for
// when a designer wants a specific line for a specific thing. so the rule everywhere in here is:
// if flavorText is filled in, use it verbatim. otherwise build something sensible ourselves.
//
// keeping all of this in one file means handlers never do string concat, and when we do start
// getting real flavor text from logic there's exactly one place to change.
public static class BattleTextBuilder
{
    private const string UNKNOWN_SHRIMP_NAME = "someone";
    private const string UNKNOWN_MOVE_NAME = "something";

    // ---------- combat ----------

    public static string Attack(BattleEvent evt, UIShrimpState source)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        return $"{NameOf(source)} used {MoveNameOf(source, evt.moveId)}!";
    }

    public static string TakeDamage(BattleEvent evt, UIShrimpState source)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        // deltaValue is the damage. logic sends it positive from attacks and negative from
        // status ticks, so just take the size of it
        int amount = Abs(evt.deltaValue);

        return $"{NameOf(source)} took {amount} damage!";
    }

    public static string Heal(BattleEvent evt, UIShrimpState source)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        return $"{NameOf(source)} recovered {Abs(evt.deltaValue)} HP!";
    }

    // ---------- statuses and abilities ----------

    public static string StatusApplied(BattleEvent evt, UIShrimpState source, StatusDefinition status)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        if (status == null)
            return $"{NameOf(source)} was affected by something.";

        if (status.effectType == TypeOfEffect.Positive)
            return $"{NameOf(source)} gained {DisplayNameOf(status)}!";

        return $"{NameOf(source)} was inflicted with {DisplayNameOf(status)}!";
    }

    public static string StatusRemoved(BattleEvent evt, UIShrimpState source, StatusDefinition status)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        if (status == null)
            return $"{NameOf(source)} is back to normal.";

        return $"{NameOf(source)} is no longer affected by {DisplayNameOf(status)}.";
    }

    public static string AbilityTriggered(BattleEvent evt, UIShrimpState source)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        AbilityDefinition ability = source != null ? source.ability : null;

        if (ability == null || string.IsNullOrEmpty(ability.displayName))
            return $"{NameOf(source)}'s ability activated!";

        return $"{NameOf(source)}'s {ability.displayName} activated!";
    }

    // ---------- roster ----------

    public static string SwitchingShrimp(BattleEvent evt, UIShrimpState incoming)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        return $"{NameOf(incoming)}, you're up!";
    }

    public static string CharacterDied(BattleEvent evt, UIShrimpState source)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        return $"{NameOf(source)} is out of the fight!";
    }

    // ---------- battle end ----------

    public static string BattleWon(BattleEvent evt)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        return "you win!";
    }

    public static string BattleLost(BattleEvent evt)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        return "you lost...";
    }

    // ---------- prompts ----------

    public static string ChoosingMove(BattleEvent evt, UIShrimpState activeShrimp)
    {
        if (HasOverride(evt))
            return evt.flavorText;

        return $"what should {NameOf(activeShrimp)} do?";
    }

    // ---------- move details panel ----------

    /// <summary>
    /// The full writeup for a move, shown in the text area while the player is picking.
    /// Damage/healing, what effect it applies, and the lore blurb.
    /// </summary>
    public static string MoveDetails(MoveDefinition move)
    {
        if (move == null)
            return string.Empty;

        // TODO: revisit alongside MoveCategory once owen settles how categories are stored
        MoveCategory category = MoveCategories.Classify(move);
        int magnitude = MoveCategories.GetMagnitude(move);

        string header = DisplayNameOf(move);
        string numbers;

        switch (category)
        {
            case MoveCategory.Healing:
                numbers = $"heals {magnitude}";
                break;

            case MoveCategory.Attack:
                numbers = $"power {magnitude}";
                break;

            default:
                numbers = "no direct damage";
                break;
        }

        string effect = string.Empty;
        if (move.hasEffect && move.effect != null)
        {
            effect = $"  |  applies {DisplayNameOf(move.effect)}";
        }

        string description = !string.IsNullOrEmpty(move.longDescription)
            ? move.longDescription
            : move.shortDescription;

        return $"<b>{header}</b>\n{numbers}{effect}\n\n{description}";
    }

    // ---------- helpers ----------

    // a designer filled flavorText in, so use their line instead of building one
    private static bool HasOverride(BattleEvent evt)
    {
        return !string.IsNullOrEmpty(evt.flavorText);
    }

    /// <summary>
    /// A shrimp's name, or a neutral stand in when we don't know who it is.
    /// Keeps lines readable rather than printing "'s ability activated!" with a blank in front.
    /// </summary>
    public static string NameOf(UIShrimpState shrimp)
    {
        if (shrimp == null || string.IsNullOrEmpty(shrimp.displayName))
            return UNKNOWN_SHRIMP_NAME;

        return shrimp.displayName;
    }

    /// <summary>
    /// Finds a move definition by id.
    ///
    /// Move assets don't live under Resources, so there's no way to look one up by id on its
    /// own. We go through the shrimp that used it instead, since it holds direct references
    /// to all of its own moves. Returns null if the move isn't one of theirs.
    /// </summary>
    public static MoveDefinition FindMove(UIShrimpState shrimp, string moveId)
    {
        if (shrimp == null || shrimp.moveData == null || string.IsNullOrEmpty(moveId))
            return null;

        foreach (MoveDefinition move in shrimp.moveData)
        {
            if (move != null && move.moveID == moveId)
                return move;
        }

        return null;
    }

    /// <summary>
    /// Finds a status definition by id, via the shrimp currently carrying it.
    ///
    /// Two things to know: <c>statusID</c> is blank on every status asset right now, so this
    /// usually can't match on anything. And for a status that just expired the logic has
    /// already pulled it off the list, so this returns null there by design.
    /// </summary>
    public static StatusDefinition FindStatus(UIShrimpState shrimp, string statusId)
    {
        if (shrimp == null || shrimp.statusEffects == null || string.IsNullOrEmpty(statusId))
            return null;

        foreach (AppliedStatus applied in shrimp.statusEffects)
        {
            if (applied != null && applied.status != null && applied.status.statusID == statusId)
                return applied.status;
        }

        return null;
    }

    // the move's pretty name if we can find it, otherwise the raw id so at least the line
    // tells you which move misbehaved
    private static string MoveNameOf(UIShrimpState shrimp, string moveId)
    {
        MoveDefinition move = FindMove(shrimp, moveId);

        if (move != null)
            return DisplayNameOf(move);

        // no matching move on the shrimp -- fall back to the raw id so it's at least debuggable
        return !string.IsNullOrEmpty(moveId) ? moveId : UNKNOWN_MOVE_NAME;
    }

    private static string DisplayNameOf(MoveDefinition move)
    {
        if (move == null)
            return UNKNOWN_MOVE_NAME;

        return !string.IsNullOrEmpty(move.displayName) ? move.displayName : move.name;
    }

    private static string DisplayNameOf(StatusDefinition status)
    {
        if (status == null)
            return "something";

        return !string.IsNullOrEmpty(status.displayName) ? status.displayName : status.name;
    }

    // damage arrives positive from attacks but negative from status ticks, and healing is
    // stored as negative power. the player shouldn't be reading "took -5 damage"
    private static int Abs(int value)
    {
        return value < 0 ? -value : value;
    }
}
