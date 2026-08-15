using System.Collections.Generic;
using UnityEngine;

// written by Claude Opus 5
// figures out whether a given shrimp id belongs to the player or the enemy.
//
// why this exists: BattleSetupData hands us one flat list of shrimp with no team marker,
// and BattleEvent only ever gives us an id. but the ui needs to know which info display to
// update. so we have to work it out from the id itself.
//
// the parsing is pluggable on purpose -- owen's id format isn't locked in yet. if it changes,
// write a new IShrimpSideParser and Register() it instead of editing every caller.
public enum BattleSide
{
    Player,
    Enemy,
    Unknown
}

/// <summary>
/// One way of reading a team out of a shrimp id.
///
/// Implement this and hand it to <c>BattleSideResolver.Register()</c> when the id format
/// changes or gains a new variant.
/// </summary>
public interface IShrimpSideParser
{
    /// <summary>
    /// Attempts to read a side out of <paramref name="shrimpId"/>.
    /// </summary>
    /// <returns>
    /// False if this parser doesn't recognise the format, which tells the resolver to move on
    /// to the next parser. Only return true when you're actually sure.
    /// </returns>
    bool TryParse(string shrimpId, out BattleSide side);
}

public static class BattleSideResolver
{
    // tried in order, first one to return true wins
    private static List<IShrimpSideParser> parsers = new() { new IdConventionSideParser() };

    // so we don't spam the console once per frame for the same bad id
    private static HashSet<string> alreadyWarned = new();

    /// <summary>
    /// Adds another way of reading a side out of a shrimp id.
    ///
    /// Parsers are tried in the order they're registered, and the built in
    /// <c>IdConventionSideParser</c> is always first unless <c>ClearParsers()</c> is called.
    /// </summary>
    public static void Register(IShrimpSideParser parser)
    {
        if (parser == null)
            return;

        parsers.Add(parser);
    }

    /// <summary>
    /// Drops every parser, including the built in one.
    ///
    /// Only worth calling if the id format changes completely and the default would start
    /// giving wrong answers rather than just no answer.
    /// </summary>
    public static void ClearParsers()
    {
        parsers.Clear();
    }

    /// <summary>
    /// Works out which team a shrimp belongs to from its id.
    /// Returns <c>BattleSide.Unknown</c> (and warns once) if nothing recognises the id.
    /// </summary>
    public static BattleSide FromId(string shrimpId)
    {
        if (string.IsNullOrEmpty(shrimpId))
        {
            WarnOnce("(empty id)");
            return BattleSide.Unknown;
        }

        foreach (IShrimpSideParser parser in parsers)
        {
            if (parser.TryParse(shrimpId, out BattleSide side))
            {
                return side;
            }
        }

        WarnOnce(shrimpId);
        return BattleSide.Unknown;
    }

    // one warning per bad id. these get asked about every frame in places, so logging on
    // every call would bury everything else in the console
    private static void WarnOnce(string shrimpId)
    {
        if (alreadyWarned.Contains(shrimpId))
            return;

        alreadyWarned.Add(shrimpId);
        Debug.LogWarning($"[BattleSideResolver] >> can't tell what team '{shrimpId}' is on. " +
            $"expected something like 'shrimp.player.1'. is instanceID actually being set?");
    }
}

/// <summary>
/// Reads the id format documented in BattleUIContract's own comments:
/// <c>shrimp.player.1</c> / <c>shrimp.enemy.3</c>.
///
/// Registered by default. Matches on the <c>.player.</c> / <c>.enemy.</c> chunk rather than
/// the whole string, so the prefix and the trailing number can both change without breaking it.
/// </summary>
public class IdConventionSideParser : IShrimpSideParser
{
    private const string PLAYER_MARKER = ".player.";
    private const string ENEMY_MARKER = ".enemy.";

    bool IShrimpSideParser.TryParse(string shrimpId, out BattleSide side)
    {
        if (shrimpId.Contains(PLAYER_MARKER))
        {
            side = BattleSide.Player;
            return true;
        }

        if (shrimpId.Contains(ENEMY_MARKER))
        {
            side = BattleSide.Enemy;
            return true;
        }

        side = BattleSide.Unknown;
        return false;
    }
}
