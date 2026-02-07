using System;
using System.Collections.Generic;
using System.Collections;

// the "API" that the game logic and UI logic will communicate with
namespace Sh.UIContract
{
    public enum BattleEventType
    {
        BattleWon,
        BattleLost,

        ChoosingMove, // switch to active character
        SwitchingShrimp,

        CharacterDied,

        StatusApplied,
        StatusRemoved,

        Heal,
        TakeDamage,
        Attack,

        LogMessage, // flavor text probably
        PlaySound,

        // junk drawer
        GenericEffect,
        CameraShake
    }


    // fields can be null. fill in only what's necessary
    // the one "god struct"
    [Serializable]
    public struct BattleEvent
    {
        public BattleEventType eventType;

        // who is involved
        public string sourceId;    // i.e. shrimp.player.1
        public string targetId;    // i.e. shrimp.enemy.3

        // what happened
        public string moveId;   // "move.punch", "status.weaken", "move.heal", etc.
        public string flavorText;  // "move.punch.effective", "move.heal", "move.status.apply", etc.

        // the change and the truth
        // use delta to change, and use final value so that ui won't be out of sync.
        public int deltaValue;
        public int finalValue;
        public int maxValue;

        // junk drawer
        // pass in extra data if needed in case there are specific mechanics that do something "out of the ordinary"
        public List<int> ints;         // i.e. multi hit move -- you tell ui how many times to hit. put 3 inside ints[0] (obv we have to coordinate on this)
        public List<float> floats;     // i.e. play some animation at 1.5x the speed. put 1.5f inside floats[0]
        public List<string> strings;   // i.e. trigger a specific sound file that isn't an id. for example, "sound.bonk" inside strings[0]
        public List<bool> bools;       // honestly, not sure what this could be for, but just in case
    }


    // battle initialization data (used only once)
    [Serializable]
    public struct BattleSetupData
    {
        public List<CharacterInitialData> playerTeam;
        public List<CharacterInitialData> enemyTeam;
    }

    // also used only once
    [Serializable]
    public struct CharacterInitialData
    {
        public string name;
        public string spriteId;
        public string pfpId;
        public int maxHp;
        public int currentHp;
        public int attack;
        public int attackSpeed;
        public List<string> effects; // abilities, status effects
    }


    public interface IBattleUI
    {
        void InitializeBattle(BattleSetupData setupData);
        void QueueEvent(BattleEvent gameEvent); // called when anything happens that require ui to react
    }


    // ui will use this to tell game logic what player wants to do
    public interface IBattleCommands
    {
        void SelectAction(string actionID, ActionType actionType);

        void Back();

        void TogglePause();

        void DialogueSkipAll();

        void DialogueConfirm();
    }


    public interface IBattleEventHandler
    {
        /// <summary>
        /// Returns true if this handler can process an event right now.
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        bool CanHandle(BattleEventType eventType);

        IEnumerator HandleEvent(BattleEvent evt);
    }


    // a dictionary of string names so typos are pretty much impossible.
    // do not clone (u can't do that anyway lmao)
    // add more stuff here when needed
    public static class BattleKeys
    {
        public static class Animation
        {
            // examples:
            // public const string ATTACK_BASIC = "anim_attack_basic";
            // public const string HIT_REACTION = "anim_hit_reaction";
            // public const string SPAWN_IN = "anim_spawn_in";
            // public const string DEATH = "anim_death";
        }

        public static class SFX
        {
            // examples:
            // public const string BONK = "sfx_bonk_01";
            // public const string CRIT = "sfx_crit_impact";
            // (that is, if we're using that list of strings in jumk drawer)
        }

        public static class Params
        {
            // examples:
            // when event is "CameraShake"
            // public const int CAM_SHAKE_INTENSITY_IDX = 0; // floats[0]
            // public const int CAM_SHAKE_DURATION_IDX = 1;  // floats[1]
        }

        public static class Paths
        {
            public const string UI_ROOT_PATH = "Art/UI/";
            public const string SFX_ROOT_PATH = "Audio/SFX/";
        }


        public static class Sprites
        {
            // example:
            // public const string SPRITE_FROG = Paths.UI_ROOT_PATH + "shrimp.player.frog";
        }

        public static class ShrimpMoves
        {
            // example:
            // public const string MOVE_SLIMYSHRIMP_BASICATTACK = Paths.UI_ROOT_PATH + "moves.slimyShrimp.basicAttack";
        }
    }
}