using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using Shrimp5.UIContract;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BattleController : MonoBehaviour, IBattleUIActions
{
    private List<List<ShrimpDefinition>> enemies;
    public List<ShrimpDefinition> enemyTeam1;
    public List<ShrimpDefinition> enemyTeam2;
    public List<ShrimpDefinition> enemyTeam3;
    public List<ShrimpDefinition> enemyTeam4;
    public List<ShrimpDefinition> enemyTeam5;
    public List<ShrimpDefinition> enemyTeam6;
    public List<ShrimpDefinition> enemyTeam7;
    public List<ShrimpDefinition> enemyTeam8;
    public List<ShrimpDefinition> enemyTeam9;
    public List<ShrimpDefinition> enemyTeam10;
    [Header("Refs")]
    [SerializeField] private BattleUIModel uiModel;
    private BattleSnapshot currentSnapshot;
    private List<ShrimpState> playerTeam;
    private List<ShrimpState> enemyTeam;
    public BattleLoopController loopController;
    private System.Random rng;
    private ShrimpState playerActiveShrimp;
    private ShrimpState enemyActiveShrimp;
    private List<ButtonData> moves;
    private List<ButtonData> switchShrimp;
    private bool frozen;
    private Queue<string> flavorTextQueue;
    private bool turnOver;
    private bool attacking;
    private bool abilityActive;
    private bool dying;
    private BattleUIMode modeBeforePause;
    private System.Action<bool> onBattleEndCallback;

    void Start()
    {
        enemies = new List<List<ShrimpDefinition>>
        {
            enemyTeam1,
            enemyTeam2,
            enemyTeam3,
            enemyTeam4,
            enemyTeam5,
            enemyTeam6,
            enemyTeam7,
            enemyTeam8,
            enemyTeam9,
            enemyTeam10
        };
    }
    public void StartNewBattle(List<ShrimpState> team, System.Action<bool> callback, int battleIndex)
    {
        turnOver = true;
        // creates the snapshot and automatically sets the battle mode to choosing action
        currentSnapshot = new BattleSnapshot();
        currentSnapshot.promptText = "it's your turn!";
        currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
        currentSnapshot.buttons = new List<ButtonData>();
        rng = new System.Random();

        // sets the active shrimp
        playerTeam = team;
        enemyTeam = new List<ShrimpState>();
        foreach (ShrimpDefinition enemyDef in enemies[battleIndex])
        {
            enemyTeam.Add(new ShrimpState(enemyDef));
        }
        // Calls the methods to set the player and enemy HudData's to the active shrimp

        foreach (ShrimpState shrimp in playerTeam)
        {
            shrimp.ResetState();
        }

        foreach (ShrimpState shrimp in enemyTeam)
        {
            shrimp.ResetState();
        }
        SetupPlayerHudData(); 
        SetupEnemyHudData(); 

        moves = new List<ButtonData>();
        // adds all of the active shrimps move data to the MoveData list for the UI
        for (int i = 0; i < playerActiveShrimp.definition.moves.Length; i++)
        {
            ButtonData currentMove = new ButtonData();
            currentMove.isEnabled = true;
            currentMove.iconID = playerActiveShrimp.definition.moves[i].iconID;
            currentMove.moveName = playerActiveShrimp.definition.moves[i].displayName;
            currentMove.moveShortDescription = playerActiveShrimp.definition.moves[i].description;
            moves.Add(currentMove);
        }

        ButtonData moveSwitchButton = new ButtonData {
        iconID = "switch_icon",
        isEnabled = true,
        moveShortDescription = "Switch"
        };

        ButtonData shrimpSwitchButton = new ButtonData {
        iconID = "switch_icon",
        isEnabled = true,
        moveShortDescription = "Back"
        };
        
        moves.Add(moveSwitchButton);
        currentSnapshot.buttons = moves;

        // Sets the inspect data to the current Move they are on
        currentSnapshot.inspectData.iconID = currentSnapshot.buttons[0].iconID;
        currentSnapshot.inspectData.title = currentSnapshot.buttons[0].moveName;
        currentSnapshot.inspectData.body = currentSnapshot.buttons[0].moveShortDescription;

        //Removes the active shrimp from the list used to store the inactive ones
        playerTeam.RemoveAt(0);
        enemyTeam.RemoveAt(0);

        switchShrimp = new List<ButtonData>();
        //Adds the data for the shrimp in the back to the moves list
        for (int i = 0; i < playerTeam.Count; i++)
        {
            ButtonData currentShrimp = new ButtonData();
            currentShrimp.isEnabled = true;
            currentShrimp.iconID = playerTeam[i].definition.shrimpSpriteID;
            currentShrimp.moveName = playerTeam[i].definition.name;
            currentShrimp.moveShortDescription = playerTeam[i].currentHP.ToString() + "/" + playerTeam[i].definition.maxHP + " HP";
            switchShrimp.Add(currentShrimp);
        }
        switchShrimp.Add(shrimpSwitchButton);
        abilityActive = true;
        // updates the UI with starting data
        frozen = true;
        flavorTextQueue = new Queue<string>();
        flavorTextQueue.Enqueue("Battle Started");
        flavorTextQueue.Enqueue("You Sent Out " + currentSnapshot.playerInfoData.teammateName);
        flavorTextQueue.Enqueue("Your Opponent Sent Out " + currentSnapshot.enemyInfoData.teammateName);
        currentSnapshot.flavorText = flavorTextQueue.Dequeue();
        UpdateUI();
    }
    /// <summary>
    /// Sets the player info data to the data of the current active shrimp when called
    /// </summary>
    private void SetupPlayerHudData()
    {
        currentSnapshot.playerInfoData.attack = playerActiveShrimp.GetAttack(); 
        currentSnapshot.playerInfoData.attackSpeed = playerActiveShrimp.GetSpeed(); 
        currentSnapshot.playerInfoData.hp = playerActiveShrimp.GetHP(); 
        currentSnapshot.playerInfoData.maxHp = playerActiveShrimp.definition.maxHP; 
        currentSnapshot.playerInfoData.teammateName = playerActiveShrimp.definition.name; 
        currentSnapshot.playerInfoData.portraitIconID = playerActiveShrimp.definition.shrimpSpriteID;
        currentSnapshot.playerInfoData.passives = new List<List<string>>(); 
        List<string> playerAbilityInfo = new List<string>();
        playerAbilityInfo.Add(playerActiveShrimp.definition.ability.iconID);
        playerAbilityInfo.Add(playerActiveShrimp.definition.ability.description);
        currentSnapshot.playerInfoData.passives.Add(playerAbilityInfo);
        UpdateUI();
    }

    /// <summary>
    /// sets the enemy info data to the data of the active enemy shrimp when called
    /// </summary>
    private void SetupEnemyHudData()
    {
        currentSnapshot.enemyInfoData.attack = enemyActiveShrimp.GetAttack();
        currentSnapshot.enemyInfoData.attackSpeed = enemyActiveShrimp.GetSpeed();
        currentSnapshot.enemyInfoData.hp = enemyActiveShrimp.GetHP();
        currentSnapshot.enemyInfoData.maxHp = enemyActiveShrimp.definition.maxHP;
        currentSnapshot.enemyInfoData.teammateName = enemyActiveShrimp.definition.name;
        currentSnapshot.enemyInfoData.portraitIconID = enemyActiveShrimp.definition.shrimpSpriteID;
        currentSnapshot.enemyInfoData.passives = new List<List<string>>();
        List<string> enemyAbilityInfo = new List<string>();
        enemyAbilityInfo.Add(enemyActiveShrimp.definition.ability.iconID);
        enemyAbilityInfo.Add(enemyActiveShrimp.definition.ability.description);
        currentSnapshot.enemyInfoData.passives.Add(enemyAbilityInfo);
        UpdateUI();
    }
    
    /// <summary>
    /// Whenever a shrimp dies or is swapped out, resets the switch options to put it in the moves list
    /// </summary>
    private void ResetPlayerSwitchOptions()
    {
       for(int i = 0; i < playerTeam.Count; i++)
        {
            ButtonData currentShrimp = new ButtonData();
            currentShrimp.isEnabled = true;
            currentShrimp.iconID = playerTeam[i].definition.shrimpSpriteID;
            currentShrimp.moveName = playerTeam[i].definition.name;
            currentShrimp.moveShortDescription = playerTeam[i].currentHP.ToString() + "/" + playerTeam[i].definition.maxHP + " HP";
            switchShrimp[i] = currentShrimp;
        }
    }

    /// <summary>
    /// sets the moves to the moves of the active shrimp for the ui
    /// </summary>
    private void ResetPlayerMoveOptions()
    {
        for (int i = 0; i < playerActiveShrimp.definition.moves.Length; i++)
        {
            ButtonData currentMove = new ButtonData();
            currentMove.isEnabled = true;
            currentMove.iconID = playerActiveShrimp.definition.moves[i].iconID;
            currentMove.moveName = playerActiveShrimp.definition.moves[i].displayName;
            currentMove.moveShortDescription = playerActiveShrimp.definition.moves[i].description;
            moves[i] = currentMove;
        }
    }

    /// <summary>
    /// sends the current snapshot to the UI
    /// </summary>
    private void UpdateUI()
    {
        uiModel.SetSnapshot(currentSnapshot);
    }
    //exits out of inspecting to go back to choosing an action
    public void Back()
    {
        if (currentSnapshot.battleMode == BattleUIMode.InspectingMove)
        {
            currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        }
        UpdateUI();
    }

    // Confirms if the player chooses a move or switches, and then runs the turn
    public void Confirm(int index)
    {
        if (!frozen)
        {
        if (currentSnapshot.battleMode == BattleUIMode.ChoosingAction)
        {
            if (index == 3)
            {
                currentSnapshot.battleMode = BattleUIMode.ChoosingSwitchTeammate;
                currentSnapshot.buttons = switchShrimp;
                UpdateUI();
            }
            else
            {
            currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
            UpdateUI();
            StartCoroutine(RunTurnCoroutine(index, ActionType.Attacking));
            }
        }
        else if (currentSnapshot.battleMode == BattleUIMode.ChoosingSwitchTeammate)
        {
            if (index == 3)
            {
                currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
                currentSnapshot.buttons = moves;
                UpdateUI();
            }
            else
            {
            currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
            UpdateUI();
            StartCoroutine(RunTurnCoroutine(index, ActionType.Switching));
            }
        }
        }
        UpdateUI();
    }

    // Represents when a player starts to inspect something
    public void Secondary(int index)
    {
    }

    // toggles pausing and unpausing the game
    public void PauseToggle()
    {
        if (currentSnapshot.battleMode == BattleUIMode.Paused)
        {
            currentSnapshot.battleMode = modeBeforePause;
        } 
        else
        {
            modeBeforePause = currentSnapshot.battleMode;
            currentSnapshot.battleMode = BattleUIMode.Paused;
        }
        UpdateUI();
    }

    // sets the target for a tooltip
    public void SetTooltipTarget(string tooltipID)
    {
        UpdateUI();
    }
    
    /// <summary>
    /// simulates a turn, deciding what actions take place in what order
    /// </summary>
    /// <param name="index"></param> The index of the move that the player used
    /// <param name="action"></param> whether the player is switching or attacking

    private IEnumerator RunTurnCoroutine(int index, ActionType action)
    {
        frozen = true;
        currentSnapshot.promptText = "Running Turn";
        turnOver = false;
        // if the player is switching it swaps what is active and then has the enemy choose an attack
        if (action == ActionType.Switching)
        {
            ShrimpState temp = playerActiveShrimp;
            playerActiveShrimp = playerTeam[index];
            playerTeam[index] = temp;
            flavorTextQueue.Enqueue("You Switched Out " + temp.name + " for " + playerActiveShrimp.definition.name);
            yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
            SetupPlayerHudData();
            ResetPlayerSwitchOptions();
            ResetPlayerMoveOptions();
            UpdateUI();
            abilityActive = true;
            StartCoroutine(OnSwitchInAbilityCoroutine(User.Player));
            yield return new WaitUntil( () => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Enemy));
            yield return new WaitUntil( () => !abilityActive);
            UpdateUI();
            attacking = true;
            StartCoroutine(EnemyAttackCoroutine(EnemyMoveSelection(index)));
            yield return new WaitUntil( () => !attacking);
        }
        // if the player is attacking it checks to see who is faster and gets to attack first
        else
        {
        int playerSpeed = playerActiveShrimp.GetSpeed();
        int enemySpeed = enemyActiveShrimp.GetSpeed();
        if (playerSpeed > enemySpeed)
        {
            abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Player));
            yield return new WaitUntil(() => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Enemy));
            yield return new WaitUntil(() => !abilityActive);
            attacking = true;
            StartCoroutine(PlayerAttackCoroutine(index));
            yield return new WaitUntil(() => !attacking);
            attacking = true;
            StartCoroutine(EnemyAttackCoroutine(EnemyMoveSelection(index)));   
            yield return new WaitUntil(() => !attacking);
            abilityActive = true;         
            StartCoroutine(OnTurnEndAbilityCoroutine(User.Player));
            yield return new WaitUntil(() => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnEndAbilityCoroutine(User.Enemy));
            yield return new WaitUntil(() => !abilityActive);
        }
        else if (playerSpeed < enemySpeed)
        {
            abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Enemy));
            yield return new WaitUntil(() => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Player));
            yield return new WaitUntil(() => !abilityActive);
            attacking = true;
            StartCoroutine(EnemyAttackCoroutine(EnemyMoveSelection(index)));
            yield return new WaitUntil(() => !attacking);
            attacking = true;
            StartCoroutine(PlayerAttackCoroutine(index));
            yield return new WaitUntil(() => !attacking);
            abilityActive = true;
            StartCoroutine(OnTurnEndAbilityCoroutine(User.Enemy));
            yield return new WaitUntil(() => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnEndAbilityCoroutine(User.Player));
            yield return new WaitUntil(() => !abilityActive);
        }
        else
        {
            int whoseTurn = rng.Next(0,2);
            if (whoseTurn == 0)
            {
                abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Player));
            yield return new WaitUntil(() => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Enemy));
            yield return new WaitUntil(() => !abilityActive);
            attacking = true;
            StartCoroutine(PlayerAttackCoroutine(index));
            yield return new WaitUntil(() => !attacking);
            attacking = true;
            StartCoroutine(EnemyAttackCoroutine(EnemyMoveSelection(index)));   
            yield return new WaitUntil(() => !attacking);
            abilityActive = true;         
            StartCoroutine(OnTurnEndAbilityCoroutine(User.Player));
            yield return new WaitUntil(() => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnEndAbilityCoroutine(User.Enemy));
            yield return new WaitUntil(() => !abilityActive);
            }
            else
            {
                abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Enemy));
            yield return new WaitUntil(() => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnStartAbilityCoroutine(User.Player));
            yield return new WaitUntil(() => !abilityActive);
            attacking = true;
            StartCoroutine(EnemyAttackCoroutine(EnemyMoveSelection(index)));
            yield return new WaitUntil(() => !attacking);
            attacking = true;
            StartCoroutine(PlayerAttackCoroutine(index));
            yield return new WaitUntil(() => !attacking);
            abilityActive = true;
            StartCoroutine(OnTurnEndAbilityCoroutine(User.Enemy));
            yield return new WaitUntil(() => !abilityActive);
            abilityActive = true;
            StartCoroutine(OnTurnEndAbilityCoroutine(User.Player));
            yield return new WaitUntil(() => !abilityActive);
            }
        }
        
        }
        if (enemyActiveShrimp.GetHP() <= 0)
        {
            dying = true;
            StartCoroutine(KillEnemyShrimpCoroutine());
            yield return new WaitUntil(() => !dying);
        }
        if (playerActiveShrimp.GetHP() <= 0)
        {
            dying = true;
            StartCoroutine(KillPlayerShrimpCoroutine());
            yield return new WaitUntil(() => !dying);
        }
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        turnOver = true;
        currentSnapshot.promptText = "Your Move!";
        DialogueSkipAll();
        UpdateUI();
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        currentSnapshot.selectedIndex = 0;
        currentSnapshot.buttons = moves;
        UpdateUI();
    }

    private IEnumerator PlayerAttackCoroutine(int index)
    {
        yield return null;
        // kills the shrimp if its HP is below zero
        if (playerActiveShrimp.GetHP() <= 0)
        {
            dying = true;
            StartCoroutine(KillPlayerShrimpCoroutine());
            yield return new WaitUntil(() => !dying);
        }
        // calculates the damage the shrimp does and deals it to the target and applies any statuses
        else
        {
            MoveDefinition move = playerActiveShrimp.definition.moves[index];
            int shrimpPower = playerActiveShrimp.GetAttack();
            int damage = shrimpPower*move.power;
            flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " used " + move.name);
            if (move.target == MoveTarget.Opponent)
            {
                enemyActiveShrimp.currentHP = enemyActiveShrimp.GetHP() - damage; 
                UpdateUI();
                if (damage > 0)
                {
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " took " + damage + " damage");
                }
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    enemyActiveShrimp.statuses.Add(newStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(newStatus.status.iconID);
                    statusInfo.Add(newStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents" + enemyActiveShrimp.definition.name + " recieved the status " + move.effect.displayName);
                    UpdateUI();
                }
                currentSnapshot.enemyInfoData.hp = enemyActiveShrimp.GetHP();
                currentSnapshot.enemyInfoData.attack = enemyActiveShrimp.GetAttack();
                currentSnapshot.enemyInfoData.attackSpeed = enemyActiveShrimp.GetSpeed();
                if (damage > 0)
                {
                    abilityActive = true;
                    StartCoroutine(OnAttackAbilityCoroutine(User.Player));
                    yield return new WaitUntil(() => !abilityActive);
                    abilityActive = true;
                    StartCoroutine(OnDamagedAbilityCoroutine(User.Enemy));
                    yield return new WaitUntil(() => !abilityActive);
                }
            }
            else
            {
                playerActiveShrimp.currentHP = playerActiveShrimp.GetHP() - damage;
                if (damage < 0)
                {
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " healed " + damage + " damage");
                }
                if (damage > 0)
                {
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " took " + damage + " damage");
                }
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    playerActiveShrimp.statuses.Add(newStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(newStatus.status.iconID);
                    statusInfo.Add(newStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    playerActiveShrimp.currentHP = playerActiveShrimp.GetHP();
                }
                currentSnapshot.playerInfoData.hp = playerActiveShrimp.currentHP;
                currentSnapshot.playerInfoData.attack = playerActiveShrimp.GetAttack();
                currentSnapshot.playerInfoData.attackSpeed = playerActiveShrimp.GetSpeed();
                flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + move.effect.displayName);
                UpdateUI();
            }
        }

        // decreases the turns on the player's statuses and removes any ones that have expired
        List<int> removedStatuses = playerActiveShrimp.UpdateStatuses();
        foreach (int i in removedStatuses)
        {
            // i + 1 because index 0 is always the shrimp's ability
            currentSnapshot.playerInfoData.passives.RemoveAt(i + 1);
        }

        //resets player and enemy stats for the UI
        currentSnapshot.playerInfoData.attack = playerActiveShrimp.GetAttack();
        currentSnapshot.playerInfoData.attackSpeed = playerActiveShrimp.GetSpeed();
        currentSnapshot.playerInfoData.hp = playerActiveShrimp.GetHP();

        currentSnapshot.enemyInfoData.attack = enemyActiveShrimp.GetAttack();
        currentSnapshot.enemyInfoData.attackSpeed = enemyActiveShrimp.GetSpeed();
        currentSnapshot.enemyInfoData.hp = enemyActiveShrimp.GetHP();
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        attacking = false;
    }
    /// <summary>
    /// Calculates the best move for the AI to use
    /// </summary>
    /// <param name="playerAttackIndex"></param> the index of the attack that the player is using
    /// <returns></returns> the index of the best attack for the enemy to use
    private int EnemyMoveSelection(int playerAttackIndex)
    {
        int enemyAttack = enemyActiveShrimp.GetAttack();
        int playerAttack = playerActiveShrimp.GetAttack();
        MoveDefinition[] enemyMoves = enemyActiveShrimp.definition.moves;
        MoveDefinition playerMove = playerActiveShrimp.definition.moves[playerAttackIndex];
        int[] moveScores = new int[3];
        int highestScoreIndex = 0;
        for (int i = 0; i <= 2; i++)
        {
            if (enemyMoves[i].power*enemyAttack >= playerActiveShrimp.currentHP)
            {
                moveScores[i] += 20;
            }
            if (enemyMoves[i].power*enemyAttack >= 0)
            {
                moveScores[i] += 5;
            }
            if ((playerMove.power*playerAttack >= enemyActiveShrimp.currentHP) && ((enemyMoves[i].hasEffect && (enemyMoves[i].effect.effectType == TypeOfEffect.Positive) && (enemyMoves[i].target == MoveTarget.Self) && (enemyMoves[0].effect.statChanged == StatAffected.HP)) || (enemyMoves[i].hasEffect && (enemyMoves[i].effect.effectType == TypeOfEffect.Negative) && (enemyMoves[i].target == MoveTarget.Opponent) && (enemyMoves[i].effect.statChanged == StatAffected.Attack))))
            {
                moveScores[i] += 6;
            }
            if ((playerMove.power*playerAttack < enemyActiveShrimp.currentHP/2) && (((enemyMoves[i].hasEffect) && (enemyMoves[i].effect.statChanged == StatAffected.Attack)) || ((enemyMoves[i].hasEffect) && (enemyMoves[i].effect.statChanged == StatAffected.Speed) && (enemyActiveShrimp.GetSpeed() <= playerActiveShrimp.GetSpeed()))))
            {
                moveScores[i] += 6;
            }
            if (moveScores[i] > moveScores[highestScoreIndex])
            {
                highestScoreIndex = i;
            }
        }
        
        return highestScoreIndex;
    }
    /// <summary>
    /// Runs the enemies attack and deals damage
    /// </summary>
    /// <param name="index"></param> the index of the attack it is using
    private IEnumerator EnemyAttackCoroutine(int index)
    {
        // if the enemies hp is below zero then it dies
        if (enemyActiveShrimp.GetHP() <= 0)
        {
            dying = true;
            StartCoroutine(KillEnemyShrimpCoroutine());
            yield return new WaitUntil(() => !dying);
        }
        // calculates the amount of damage the enemy should deal and then deals it
        else
        {
            MoveDefinition move = enemyActiveShrimp.definition.moves[index];
            int shrimpPower = enemyActiveShrimp.GetAttack();
            int damage = shrimpPower*move.power;
            flavorTextQueue.Enqueue("Your opponents  " + enemyActiveShrimp.definition.name + " used " + move.name);
            if (move.target == MoveTarget.Opponent)
            {
                playerActiveShrimp.currentHP = playerActiveShrimp.GetHP() - damage;
                UpdateUI();
                if (damage > 0)
                {
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " took " + damage + " damage");
                }
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    playerActiveShrimp.statuses.Add(newStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(newStatus.status.iconID);
                    statusInfo.Add(newStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + move.effect.displayName);
                }
                currentSnapshot.playerInfoData.hp = playerActiveShrimp.GetHP();
                currentSnapshot.playerInfoData.attack = playerActiveShrimp.GetAttack();
                currentSnapshot.playerInfoData.attackSpeed = playerActiveShrimp.GetSpeed();
                UpdateUI();
                
                if (damage > 0)
                {
                    abilityActive = true;
                    StartCoroutine(OnAttackAbilityCoroutine(User.Enemy));
                    yield return new WaitUntil(() => !abilityActive);
                    abilityActive = true;
                    StartCoroutine(OnDamagedAbilityCoroutine(User.Player));
                    yield return new WaitUntil(() => !abilityActive);
                }
            }
            else
            {
                enemyActiveShrimp.currentHP = enemyActiveShrimp.GetHP() - damage;
                if (damage < 0)
                {
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " healed " + damage + " damage");
                }
                if (damage > 0)
                {
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " took " + damage + " damage");
                }
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    enemyActiveShrimp.statuses.Add(newStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(newStatus.status.iconID);
                    statusInfo.Add(newStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + move.effect.displayName);
                }
                enemyActiveShrimp.currentHP = enemyActiveShrimp.GetHP();
                currentSnapshot.enemyInfoData.hp = enemyActiveShrimp.currentHP;
                currentSnapshot.enemyInfoData.attack = enemyActiveShrimp.GetAttack();
                currentSnapshot.enemyInfoData.attackSpeed = enemyActiveShrimp.GetSpeed();
                UpdateUI();
            }
        // updates statuses
        List<int> removedStatuses = enemyActiveShrimp.UpdateStatuses();
        foreach (int i in removedStatuses)
        {
            // i + 1 because index 0 is always the shrimp's ability
            currentSnapshot.enemyInfoData.passives.RemoveAt(i + 1);
        }

        // updates the snapshot on both the player and enemy stats
        currentSnapshot.playerInfoData.attack = playerActiveShrimp.GetAttack();
        currentSnapshot.playerInfoData.attackSpeed = playerActiveShrimp.GetSpeed();
        currentSnapshot.playerInfoData.hp = playerActiveShrimp.GetHP();

        currentSnapshot.enemyInfoData.attack = enemyActiveShrimp.GetAttack();
        currentSnapshot.enemyInfoData.attackSpeed = enemyActiveShrimp.GetSpeed();
        currentSnapshot.enemyInfoData.hp = enemyActiveShrimp.GetHP();
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        attacking = false;
        }
    }

    /// <summary>
    /// Kills the active player shrimp and replaces it with the next one in the party, makes sure it cannot be used again in battle
    /// </summary>
    private IEnumerator KillPlayerShrimpCoroutine()
    {
        flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " died ");
        abilityActive = true;
        StartCoroutine(OnDeathAbilityCoroutine(User.Player));
        yield return new WaitUntil(() => !abilityActive);
        if (playerTeam.Count == 0)
        {
            flavorTextQueue.Enqueue("You Lost the Battle");
            yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
            EndBattle(false);
        }
        else
        {
        playerActiveShrimp = playerTeam[0];
        playerTeam.RemoveAt(0);
        SetupPlayerHudData();
        UpdateUI();
        flavorTextQueue.Enqueue("You sent out " + playerActiveShrimp.definition.name);
        abilityActive = true;
        StartCoroutine(OnSwitchInAbilityCoroutine(User.Player));
        yield return new WaitUntil(() => !abilityActive);
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        UpdateUI();
        dying = false;
        attacking = false;
        }
    }
    /// <summary>
    /// kills the enemy shrimp and sends out the next one
    /// </summary>
    private IEnumerator KillEnemyShrimpCoroutine()
    {
        flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " died ");
        abilityActive = true;
        StartCoroutine(OnDeathAbilityCoroutine(User.Enemy));
        yield return new WaitUntil(() => !abilityActive);
        if (enemyTeam.Count == 0)
        {
            flavorTextQueue.Enqueue("You Lost the Battle");
            yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
            EndBattle(true);
        }
        enemyActiveShrimp = enemyTeam[0];
        enemyTeam.RemoveAt(0);
        SetupEnemyHudData();
        UpdateUI();
        flavorTextQueue.Enqueue("Your opponent sent out " + enemyActiveShrimp.definition.name);
        abilityActive = true;
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        StartCoroutine(OnSwitchInAbilityCoroutine(User.Enemy));
        yield return new WaitUntil(() => !abilityActive);
        UpdateUI();
        dying = false;
        attacking = false;
    }

    public void EndBattle(bool playerWon)
    {
        // ... existing cleanup code ...
        
        // Tell the manager we are done
        onBattleEndCallback?.Invoke(playerWon);
    }

    // confirms dialogue options
    public void DialogueConfirm()
    {
        if (flavorTextQueue.Count <= 0)
        {
            // add this line! clear the text so we know the player is done reading
            currentSnapshot.flavorText = ""; 
            
            if (turnOver == true)
            {
                currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
                frozen = false;
            }
        }
        else
        {
            currentSnapshot.flavorText = flavorTextQueue.Dequeue();
        }
        UpdateUI();
    }
    // skips all dialogue
    public void DialogueSkipAll()
    {
        while (!(flavorTextQueue.Count == 0))
        {
           currentSnapshot.flavorText = flavorTextQueue.Dequeue(); 
        }
        if (turnOver)
        {
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        frozen = false;
        }
        UpdateUI();
    }

    // methods for ability triggers
    private IEnumerator OnAttackAbilityCoroutine(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnAttack)
            {
                flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnAttack)
            {
                flavorTextQueue.Enqueue("Your opponents" + enemyActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        abilityActive = false;
    }
    private IEnumerator OnDamagedAbilityCoroutine(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnDamaged)
            {
                flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnDamaged)
            {
                flavorTextQueue.Enqueue("Your opponents" + enemyActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        abilityActive = false;
        }
    private IEnumerator OnSwitchInAbilityCoroutine(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                flavorTextQueue.Enqueue("Your opponents" + enemyActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        abilityActive = false;
    }
    private IEnumerator OnTurnStartAbilityCoroutine(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnTurnStart)
            {
                flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnTurnStart)
            {
                flavorTextQueue.Enqueue("Your opponents" + enemyActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        abilityActive = false;
    }
    private IEnumerator OnTurnEndAbilityCoroutine(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnTurnEnd)
            {
                flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnTurnEnd)
            {
                flavorTextQueue.Enqueue("Your opponents" + enemyActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        abilityActive = false;
    }
    private IEnumerator OnDeathAbilityCoroutine(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnDeath)
            {
                flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnDeath)
            {
                flavorTextQueue.Enqueue("Your opponents" + enemyActiveShrimp.definition.name + "'s ability, " + ability.name + ", triggered!");
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your opponents " + enemyActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                    flavorTextQueue.Enqueue("Your " + playerActiveShrimp.definition.name + " recieved the status " + ability.effect.displayName);
                }
            }
        }
        UpdateUI();
        yield return new WaitUntil(() => flavorTextQueue.Count <= 0 && currentSnapshot.flavorText == "");
        abilityActive = false;
    }
}

public enum ActionType
{
    Switching, Attacking
}
public enum User
{
    Player, Enemy
}
