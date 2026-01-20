using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using Shrimp5.UIContract;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class BattleController : MonoBehaviour, IBattleUIActions
{
    [Header("Refs")]
    [SerializeField] private BattleUIModel uiModel;
    private BattleSnapshot currentSnapshot;
    [SerializeField] private List<ShrimpState> playerTeam;
    [SerializeField] private List<ShrimpState> enemyTeam;
    private System.Random rng;
    private ShrimpState playerActiveShrimp;
    private ShrimpState enemyActiveShrimp;
    void Start()
    {
        // creates the snapshot and automatically sets the battle mode to choosing action
        currentSnapshot = new BattleSnapshot();
        currentSnapshot.promptText = "it's your turn!";
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        currentSnapshot.buttons = new List<ButtonData>();
        rng = new System.Random();

        // sets the active shrimp
        playerActiveShrimp = playerTeam[0];
        playerActiveShrimp.statuses = new List<AppliedStatus>();
        enemyActiveShrimp = enemyTeam[0];
        enemyActiveShrimp.statuses = new List<AppliedStatus>();

        // Calls the methods to set the player and enemy HudData's to the active shrimp
        SetupPlayerHudData(); 
        SetupEnemyHudData(); 

        // adds all of the active shrimps move data to the MoveData list for the UI
        for (int i = 0; i < playerActiveShrimp.definition.moves.Length; i++)
        {
            ButtonData currentMove = new ButtonData();
            currentMove.isEnabled = true;
            currentMove.iconID = playerActiveShrimp.definition.moves[i].iconID;
            currentMove.moveName = playerActiveShrimp.definition.moves[i].displayName;
            currentMove.moveShortDescription = playerActiveShrimp.definition.moves[i].description;
            currentSnapshot.buttons.Add(currentMove); 
        }
        ButtonData switchButton = new ButtonData();
        switchButton.iconID = "Replace Laterrr r r r r ejfgfhuesvgfhjuiudhgvgbhcfikjuhgdbhiodckijubgwvhyjubighbgfviuiawu;opov";
        switchButton.isEnabled = true;
        switchButton.moveShortDescription = "switch or whatever";
        currentSnapshot.buttons.Add(switchButton);

        // Sets the inspect data to the current Move they are on
        currentSnapshot.inspectData.iconID = currentSnapshot.buttons[0].iconID;
        currentSnapshot.inspectData.title = currentSnapshot.buttons[0].moveName;
        currentSnapshot.inspectData.body = currentSnapshot.buttons[0].moveShortDescription;

        //Removes the active shrimp from the list used to store the inactive ones
        playerTeam.RemoveAt(0);
        enemyTeam.RemoveAt(0);

        // Adds the data for the shrimp in the back to the moves list
        // for (int i = 0; i < playerTeam.Count; i++)
        // {
        //     MoveData currentShrimp = new MoveData();
        //     currentShrimp.isEnabled = true;
        //     currentShrimp.iconID = playerTeam[i].definition.shrimpSpriteID;
        //     currentShrimp.moveName = playerTeam[i].definition.name;
        //     currentShrimp.moveShortDescription = playerTeam[i].definition.maxHP.ToString();
        //     currentSnapshot.moves.Add(currentShrimp);
        // }
        foreach (ButtonData move in currentSnapshot.buttons)
        {
            Debug.Log(move.moveName);
        }
        OnSwitchInAbility(User.Player);
        OnSwitchInAbility(User.Enemy);

        // updates the UI with starting data
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
            currentShrimp.moveShortDescription = playerTeam[i].definition.maxHP.ToString();
            currentSnapshot.buttons[i+3] = currentShrimp;
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
            currentSnapshot.buttons[i] = currentMove;
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
        if (currentSnapshot.battleMode == BattleUIMode.ChoosingAction)
        {
            if (index == 3)
            {
                currentSnapshot.battleMode = BattleUIMode.ChoosingSwitchTeammate;
                UpdateUI();
            }
            else
            {
            currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
            UpdateUI();
            RunTurn(index, ActionType.Attacking);
            }
        }
        else if (currentSnapshot.battleMode == BattleUIMode.ChoosingSwitchTeammate)
        {
            if (index == 3)
            {
                currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
                UpdateUI();
            }
            else
            {
            currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
            UpdateUI();
            RunTurn(index, ActionType.Switching);
            }
        }
        UpdateUI();
    }

    // Represents when a player starts to inspect something
    public void Secondary(int index)
    {
            if (currentSnapshot.battleMode == BattleUIMode.ChoosingAction)
            {
            currentSnapshot.inspectData.iconID = currentSnapshot.buttons[index].iconID;
            currentSnapshot.inspectData.title = currentSnapshot.buttons[index].moveName;
            currentSnapshot.inspectData.body = currentSnapshot.buttons[index].moveShortDescription;
            }
            if (currentSnapshot.battleMode == BattleUIMode.ChoosingSwitchTeammate)
            {
            currentSnapshot.inspectData.iconID = currentSnapshot.buttons[index+3].iconID;
            currentSnapshot.inspectData.title = currentSnapshot.buttons[index+3].moveName;
            currentSnapshot.inspectData.body = currentSnapshot.buttons[index+3].moveShortDescription;
            }
            currentSnapshot.battleMode = BattleUIMode.InspectingMove;
            UpdateUI();
        
    }

    // toggles pausing and unpausing the game
    public void PauseToggle()
    {
        if (currentSnapshot.battleMode == BattleUIMode.Paused)
        {
            currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        } 
        else
        {
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
    public void RunTurn(int index, ActionType action)
    {
        // if the player is switching it swaps what is active and then has the enemy choose an attack
        if (action == ActionType.Switching)
        {
            ShrimpState temp = playerActiveShrimp;
            playerActiveShrimp = playerTeam[index];
            playerTeam[index] = temp;
            SetupPlayerHudData();
            ResetPlayerSwitchOptions();
            ResetPlayerMoveOptions();
            UpdateUI();
            OnSwitchInAbility(User.Player);
            OnTurnStartAbility(User.Enemy);
            EnemyAttack(EnemyMoveSelection(index));
        }
        // if the player is attacking it checks to see who is faster and gets to attack first
        else
        {
        int playerSpeed = playerActiveShrimp.GetSpeed();
        int enemySpeed = enemyActiveShrimp.GetSpeed();
        if (playerSpeed > enemySpeed)
        {
            OnTurnStartAbility(User.Player);
            OnTurnStartAbility(User.Enemy);
            PlayerAttack(index);
            EnemyAttack(EnemyMoveSelection(index));
            OnTurnEndAbility(User.Player);
            OnTurnEndAbility(User.Enemy);
        }
        else if (playerSpeed < enemySpeed)
        {
            OnTurnStartAbility(User.Enemy);
            OnTurnStartAbility(User.Player);
            EnemyAttack(EnemyMoveSelection(index));
            PlayerAttack(index);
            OnTurnEndAbility(User.Enemy);
            OnTurnEndAbility(User.Player);
        }
        else
        {
            int whoseTurn = rng.Next(0,2);
            if (whoseTurn == 0)
            {
                OnTurnStartAbility(User.Player);
                OnTurnStartAbility(User.Enemy);
                PlayerAttack(index);
                EnemyAttack(EnemyMoveSelection(index));
                OnTurnEndAbility(User.Player);
                OnTurnEndAbility(User.Enemy);
            }
            else
            {
                OnTurnStartAbility(User.Enemy);
                OnTurnStartAbility(User.Player);
                EnemyAttack(EnemyMoveSelection(index));
                PlayerAttack(index);
                OnTurnEndAbility(User.Enemy);
                OnTurnEndAbility(User.Player);  
            }
        }
        }
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        currentSnapshot.selectedIndex = 0;
        UpdateUI();
    }

    private void PlayerAttack(int index)
    {
        // kills the shrimp if its HP is below zero
        if (playerActiveShrimp.GetHP() <= 0)
        {
            KillPlayerShrimp();
        }
        // calculates the damage the shrimp does and deals it to the target and applies any statuses
        else
        {
            MoveDefinition move = playerActiveShrimp.definition.moves[index];
            int shrimpPower = playerActiveShrimp.GetAttack();
            int damage = shrimpPower*move.power;
            if (move.target == MoveTarget.Opponent)
            {
                enemyActiveShrimp.currentHP -= damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    enemyActiveShrimp.statuses.Add(newStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(newStatus.status.iconID);
                    statusInfo.Add(newStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
                if (damage > 0)
                {
                    OnDamagedAbility(User.Enemy);
                }
                OnAttackAbility(User.Player);
            }
            else
            {
                playerActiveShrimp.currentHP -= damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    playerActiveShrimp.statuses.Add(newStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(newStatus.status.iconID);
                    statusInfo.Add(newStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
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
    private void EnemyAttack(int index)
    {
        // if the enemies hp is below zero then it dies
        if (enemyActiveShrimp.GetHP() <= 0)
        {
            KillEnemyShrimp();
        }
        // calculates the amount of damage the enemy should deal and then deals it
        else
        {
            MoveDefinition move = enemyActiveShrimp.definition.moves[index];
            int shrimpPower = enemyActiveShrimp.GetAttack();
            int damage = shrimpPower*move.power;

            if (move.target == MoveTarget.Opponent)
            {
                playerActiveShrimp.currentHP = playerActiveShrimp.GetHP() - damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    playerActiveShrimp.statuses.Add(newStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(newStatus.status.iconID);
                    statusInfo.Add(newStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
                if (damage > 0)
                {
                    OnDamagedAbility(User.Player);
                }
                OnAttackAbility(User.Enemy);
            }
            else
            {
                enemyActiveShrimp.currentHP -= damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    enemyActiveShrimp.statuses.Add(newStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(newStatus.status.iconID);
                    statusInfo.Add(newStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
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
        }
    }

    /// <summary>
    /// Kills the active player shrimp and replaces it with the next one in the party, makes sure it cannot be used again in battle
    /// </summary>
    private void KillPlayerShrimp()
    {
        OnDeathAbility(User.Player);
        ShrimpState temp = playerActiveShrimp;
        playerActiveShrimp = playerTeam[0];
        playerTeam.RemoveAt(0);
        playerTeam.Add(temp);
        SetupPlayerHudData();
        ResetPlayerSwitchOptions();
        ResetPlayerMoveOptions();
        ButtonData deadShrimp = currentSnapshot.buttons[2 + playerTeam.Count];
        deadShrimp.isEnabled = false;
        playerTeam.RemoveAt(playerTeam.Count-1);
        UpdateUI();
        OnSwitchInAbility(User.Player);
        UpdateUI();
        

    }
    /// <summary>
    /// kills the enemy shrimp and sends out the next one
    /// </summary>
    private void KillEnemyShrimp()
    {
        OnDeathAbility(User.Enemy);
        enemyActiveShrimp = enemyTeam[0];
        enemyTeam.RemoveAt(0);
        SetupEnemyHudData();
        UpdateUI();
        OnSwitchInAbility(User.Enemy);
        UpdateUI();

    }
    // confirms dialogue options
    public void DialogueConfirm()
    {
        currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
    }
    // skips all dialogue
    public void DialogueSkipAll()
    {
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
    }

    // methods for ability triggers
    private void OnAttackAbility(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnAttack)
            {
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnAttack)
            {
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
            }
        }
    }
    private void OnDamagedAbility(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnDamaged)
            {
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnDamaged)
            {
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
            }
        }
        }
    private void OnSwitchInAbility(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
            }
        }
    }
    private void OnTurnStartAbility(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnTurnStart)
            {
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnTurnStart)
            {
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
            }
        }
    }
    private void OnTurnEndAbility(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnTurnEnd)
            {
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnTurnEnd)
            {
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
            }
        }
    }
    private void OnDeathAbility(User user)
    {
        if (user == User.Player)
        {
            AbilityDefinition ability = playerActiveShrimp.definition.ability;
            if (ability.trigger == AbilityTrigger.OnDeath)
            {
                AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
                if (ability.target == Target.Self)
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
                else
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
            }
        }
        else
        {
            AbilityDefinition ability = enemyActiveShrimp.definition.ability;
            AppliedStatus abilityStatus = new AppliedStatus(ability.effect, ability.effect.turnDuration);
            if (ability.trigger == AbilityTrigger.OnDeath)
            {
                if (ability.target == Target.Self)
                {
                    enemyActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.enemyInfoData.passives.Add(statusInfo);
                }
                else
                {
                    playerActiveShrimp.statuses.Add(abilityStatus);
                    List<string> statusInfo = new List<string>();
                    statusInfo.Add(abilityStatus.status.iconID);
                    statusInfo.Add(abilityStatus.status.description);
                    currentSnapshot.playerInfoData.passives.Add(statusInfo);
                }
            }
        }
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
