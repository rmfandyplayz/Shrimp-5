using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;

// the "API" that the game logic and UI logic will communicate with
namespace Shrimp5.UIContract
{
    // expand only if necessary
    public enum BattleUIMode
    {
        ChoosingAction,
        InspectingMove,
        ChoosingSwitchTeammate,
        ResolvingAction,
        Paused
    }

    // use ids instead of unity assets so UI and gameplay can work independently
    // ui will map these ids to designated folders (Art/UI)
    [Serializable]
    public struct HudData
    {
        public string teammateName;

        public int hp;
        public int maxHp;

        public int attack;
        public int attackSpeed;

        public string portraitIconID;  // i.e. "stallion_shrimp"
        public List<List<string>> passives; // contains a list of status effects & abilites. as in, their icons and descriptions
                                            // example: [ ["icons.statusEffects.weaken", "Weaken: Enemy's attack has been reduced by {cumulativeWeakenAmount}"],
                                            // ["icons.statusEffects.slow", "Slow: Enemy's attack speed has been reduced by {cumulativeSlowAmount}"]]
    }

    // these count as one slot in the bottom row
    [Serializable]
    public struct MoveData
    {
        public bool isEnabled; // enable a move? enable a teammate to be selected?
        public string iconID;    // something like "move_basic_attack"
        public string moveName;  // "Basic Attack" for example
        public string moveShortDescription;    // "8 DMG, 20% to weaken by 50%"

        // hint texts
        public string hintText; // "Z to use, X to go back"

        public override string ToString()
        {
            return $"moveName: {moveName}, moveShortDescription: {moveShortDescription}, hintText: probably null, isEnabled: {isEnabled}, iconID: {iconID}";
        }
    }

    [Serializable]
    public struct InspectData // the stuff that displays when a player wants to learn more about a move
    {
        public string iconID;
        public string title;
        public string body;
    }

    [Serializable]
    public struct TooltipData
    {
        public bool isVisible;
        public string text;  // i.e. "Attack Speed is reduced by 6.7"
    }


    [Serializable]
    public struct BattleSnapshot // the stuff that will be sent to UI code to re-render everything
    {
        public BattleUIMode battleMode;
        public string promptText; // i.e. "your move!", "pick a shrimp"
        public string flavorText; // i.e. "you used your mother, dealing 20 damage to the enemy, and healing yourself for 50."

        public int selectedIndex; // a way to force the selection box to go to a certain option. probably wont be used that much

        public HudData playerInfoData;
        public HudData enemyInfoData;

        public List<MoveData> moves; // usually 4 max, but just in case

        public InspectData inspectData;

        public TooltipData tooltipData;
    }


    // helpful for animations. a small event queue ui can utilize
    public enum BattleUIEventType
    {
        ModeChanged,
        HpChanged,
        StatusAdded,
        StatusRemoved,
        CommandListChanged,
        FlavorTextChanged,
        AttackChanged,
        AttackSpeedChanged
    }


    [Serializable]
    public struct BattleUIEvent // see below for an explanation on what this is
    {
        public BattleUIEventType eventType;

        public string target;
        public string info;
        public int oldValue;
        public int newValue;
    }
    /*
     * BattleUIEvent is a "hint" from gameplay to UI about what just happened.
     * 
     * this is NOT a source-of-truth data -- BattleSnapshot is.
     * ui events exist only to help trigger animations, transitions, or
     * give more effects to things (i.e. hp bar animations, panel slide, text pop)
     * 
     * if an event is ignored at all, ui should still render correctly via
     * using the snapshot
     * 
     * what these fields mean:
     * - target & info: they tell WHAT changed (player's HP, enemy's ATK)
     * - oldValue & newValue: describes HOW it changed (player's HP goes from 50 -> 20)
     */


    public interface IBattleUIModel
    {
        event Action Changed;

        // "what does the battle look like at this very moment?"
        BattleSnapshot GetSnapshot();

        // THIS IS MORE OPTIONAL IF ANYTHING: "what just happened since last time?"
        // ui will call this after Changed to get potential animation cues
        List<BattleUIEvent> DrainUIEvents();
    }


    public interface IBattleUIActions
    {
        void Confirm(int index); // confirm a selection (i.e. use a move) (z button)
        void Secondary(int index); // the c button, which does different things under different contexts
        void Back(); // x button
        void PauseToggle();
        void SetTooltipTarget(string tooltipID);

        void DialogueConfirm(); // go to the next dialogue after the current one finished its animation (Z)
        void DialogueSkipAll(); // skip all queued dialogue animations and go straight to the next event (C)
    }
}