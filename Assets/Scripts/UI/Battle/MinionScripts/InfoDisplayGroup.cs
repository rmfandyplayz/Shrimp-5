using UnityEngine;

// written by Claude Opus 5
// tiny router in front of the two ShrimpInfoDisplays.
//
// handlers get an id off a BattleEvent and need the display that's showing that shrimp.
// without this, every single handler would be doing the same "work out the side, then pick
// player or enemy" dance.
public class InfoDisplayGroup : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private ShrimpInfoDisplay playerDisplay;
    [SerializeField] private ShrimpInfoDisplay enemyDisplay;

    /// <summary>
    /// The display currently showing this shrimp, or null if neither is.
    ///
    /// Tries the bound ids first (exact, and works even when the id format changes on us),
    /// then falls back to <c>BattleSideResolver</c> for shrimp that haven't been bound yet.
    /// </summary>
    public ShrimpInfoDisplay Get(string shrimpId)
    {
        if (string.IsNullOrEmpty(shrimpId))
            return null;

        // whoever is actually showing this shrimp right now wins
        if (playerDisplay != null && playerDisplay.GetBoundShrimpId() == shrimpId)
            return playerDisplay;

        if (enemyDisplay != null && enemyDisplay.GetBoundShrimpId() == shrimpId)
            return enemyDisplay;

        // not on screen -- fall back to parsing which team it's on. this is the path for a
        // shrimp being switched IN, since it isn't bound anywhere yet
        return GetBySide(BattleSideResolver.FromId(shrimpId));
    }

    /// <summary>
    /// The display for a team, regardless of who's currently bound to it.
    /// Returns null for <c>BattleSide.Unknown</c>.
    /// </summary>
    public ShrimpInfoDisplay GetBySide(BattleSide side)
    {
        switch (side)
        {
            case BattleSide.Player:
                return playerDisplay;

            case BattleSide.Enemy:
                return enemyDisplay;

            default:
                return null;
        }
    }

    public ShrimpInfoDisplay GetPlayerDisplay()
    {
        return playerDisplay;
    }

    public ShrimpInfoDisplay GetEnemyDisplay()
    {
        return enemyDisplay;
    }

    /// <summary>
    /// True when the given shrimp is the one the player is currently controlling.
    /// </summary>
    public bool IsPlayerSide(string shrimpId)
    {
        return Get(shrimpId) == playerDisplay && playerDisplay != null;
    }
}
