using System;
using System.Collections.Generic;
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
        currentSnapshot = new BattleSnapshot();
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;

        playerTeam = new List<ShrimpState>();
        enemyTeam = new List<ShrimpState>();
        rng = new System.Random();

        playerActiveShrimp = playerTeam[0];
        enemyActiveShrimp = enemyTeam[0]; 

        SetupPlayerHudData(); // player data is implemented
        SetupEnemyHudData(); // enemy data is implemented

        // moves are implemented
        for (int i = 0; i < playerActiveShrimp.definition.moves.Length; i++)
        {
            MoveData currentMove = new MoveData();
            currentMove.isEnabled = true;
            currentMove.iconID = playerActiveShrimp.definition.moves[i].iconID;
            currentMove.moveName = playerActiveShrimp.definition.moves[i].displayName;
            currentMove.moveShortDescription = playerActiveShrimp.definition.moves[i].description;
            currentSnapshot.moves.Add(currentMove); 
        }
        
        currentSnapshot.inspectData.iconID = currentSnapshot.moves[currentSnapshot.selectedIndex].iconID;
        currentSnapshot.inspectData.title = currentSnapshot.moves[currentSnapshot.selectedIndex].moveName;
        currentSnapshot.inspectData.body = currentSnapshot.moves[currentSnapshot.selectedIndex].moveShortDescription;

        playerTeam.RemoveAt(0);
        enemyTeam.RemoveAt(0);

        for (int i = 0; i < playerTeam.Count; i++)
        {
            MoveData currentShrimp = new MoveData();
            currentShrimp.isEnabled = true;
            currentShrimp.iconID = playerTeam[i].definition.shrimpSpriteID;
            currentShrimp.moveName = playerTeam[i].definition.name;
            currentShrimp.moveShortDescription = playerTeam[i].definition.maxHP.ToString();
            currentSnapshot.moves.Add(currentShrimp);
        }
        
        UpdateUI();
    }

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
    
    private void ResetPlayerSwitchOptions()
    {
       for(int i = 0; i < playerTeam.Count; i++)
        {
            MoveData currentShrimp = new MoveData();
            currentShrimp.isEnabled = true;
            currentShrimp.iconID = playerTeam[i].definition.shrimpSpriteID;
            currentShrimp.moveName = playerTeam[i].definition.name;
            currentShrimp.moveShortDescription = playerTeam[i].definition.maxHP.ToString();
            currentSnapshot.moves[i+3] = currentShrimp;
        }
    }

    private void ResetPlayerMoveOptions()
    {
        for (int i = 0; i < playerActiveShrimp.definition.moves.Length; i++)
        {
            MoveData currentMove = new MoveData();
            currentMove.isEnabled = true;
            currentMove.iconID = playerActiveShrimp.definition.moves[i].iconID;
            currentMove.moveName = playerActiveShrimp.definition.moves[i].displayName;
            currentMove.moveShortDescription = playerActiveShrimp.definition.moves[i].description;
            currentSnapshot.moves[i] = currentMove;
        }
    }

    private void UpdateUI()
    {
        uiModel.SetSnapshot(currentSnapshot);
    }
    public void Back()
    {
        if (currentSnapshot.battleMode == BattleUIMode.InspectingMove)
        {
            currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        }
        UpdateUI();
    }

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

    public void Secondary(int index)
    {
        currentSnapshot.battleMode = BattleUIMode.InspectingMove;
        UpdateUI();
    }

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

    public void SetTooltipTarget(string tooltipID)
    {
        UpdateUI();
    }
    
    public void RunTurn(int index, ActionType action)
    {
        if (action == ActionType.Switching)
        {
            ShrimpState temp = playerActiveShrimp;
            playerActiveShrimp = playerTeam[index];
            playerTeam[index] = temp;
            SetupPlayerHudData();
            ResetPlayerSwitchOptions();
            ResetPlayerMoveOptions();
            UpdateUI();
        }
        else
        {
        int playerSpeed = playerActiveShrimp.GetSpeed();
        int enemySpeed = enemyActiveShrimp.GetSpeed();
        if (playerSpeed > enemySpeed)
        {
            PlayerAttack(index);
            EnemyAttack(EnemyMoveSelection(index));
        }
        else if (playerSpeed < enemySpeed)
        {
            EnemyAttack(EnemyMoveSelection(index));
            PlayerAttack(index);
        }
        else
        {
            int whoseTurn = rng.Next(0,2);
            if (whoseTurn == 0)
            {
                PlayerAttack(index);
                EnemyAttack(EnemyMoveSelection(index));
            }
            else
            {
                EnemyAttack(EnemyMoveSelection(index));
                PlayerAttack(index);    
            }
        }
        }
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
        currentSnapshot.selectedIndex = 0;
        UpdateUI();
    }

    private void PlayerAttack(int index)
    {
        if (playerActiveShrimp.currentHP <= 0)
        {
            KillPlayerShrimp();
        }
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
                }
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

        List<int> removedStatuses = playerActiveShrimp.UpdateStatuses();
        foreach (int i in removedStatuses)
        {
            // i + 1 because index 0 is always the shrimp's ability
            currentSnapshot.playerInfoData.passives.RemoveAt(i + 1);
        }
        currentSnapshot.playerInfoData.attack = playerActiveShrimp.GetAttack();
        currentSnapshot.playerInfoData.attackSpeed = playerActiveShrimp.GetSpeed();
        currentSnapshot.playerInfoData.hp = playerActiveShrimp.GetHP();

        currentSnapshot.enemyInfoData.attack = enemyActiveShrimp.GetAttack();
        currentSnapshot.enemyInfoData.attackSpeed = enemyActiveShrimp.GetSpeed();
        currentSnapshot.enemyInfoData.hp = enemyActiveShrimp.GetHP();
        UpdateUI();
    }

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
    private void EnemyAttack(int index)
    {
        if (enemyActiveShrimp.currentHP <= 0)
        {
            KillEnemyShrimp();
        }
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
                }
            }
            else
            {
                enemyActiveShrimp.currentHP -= damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    enemyActiveShrimp.statuses.Add(newStatus);
                }
            }

        List<int> removedStatuses = enemyActiveShrimp.UpdateStatuses();
        foreach (int i in removedStatuses)
        {
            // i + 1 because index 0 is always the shrimp's ability
            currentSnapshot.playerInfoData.passives.RemoveAt(i + 1);
        }

        currentSnapshot.playerInfoData.attack = playerActiveShrimp.GetAttack();
        currentSnapshot.playerInfoData.attackSpeed = playerActiveShrimp.GetSpeed();
        currentSnapshot.playerInfoData.hp = playerActiveShrimp.GetHP();

        currentSnapshot.enemyInfoData.attack = enemyActiveShrimp.GetAttack();
        currentSnapshot.enemyInfoData.attackSpeed = enemyActiveShrimp.GetSpeed();
        currentSnapshot.enemyInfoData.hp = enemyActiveShrimp.GetHP();
        UpdateUI();
        }
    }
    private void KillPlayerShrimp()
    {
        ShrimpState temp = playerActiveShrimp;
        playerActiveShrimp = playerTeam[0];
        playerTeam.RemoveAt(0);
        playerTeam.Add(temp);
        SetupPlayerHudData();
        ResetPlayerSwitchOptions();
        ResetPlayerMoveOptions();
        MoveData deadShrimp = currentSnapshot.moves[2 + playerTeam.Count];
        deadShrimp.isEnabled = false;
        playerTeam.RemoveAt(playerTeam.Count-1);
        

    }
    private void KillEnemyShrimp()
    {
        enemyActiveShrimp = enemyTeam[0];
        enemyTeam.RemoveAt(0);
        SetupEnemyHudData();
        UpdateUI();
    }
    public void DialogueConfirm()
    {
        currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
    }

    public void DialogueSkipAll()
    {
        currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
    }
}

public enum ActionType
{
    Switching, Attacking
}
