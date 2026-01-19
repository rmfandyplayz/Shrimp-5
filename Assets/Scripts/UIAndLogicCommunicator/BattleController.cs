using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using Shrimp5.UIContract;
using UnityEngine;

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
        playerTeam.RemoveAt(0);
        enemyTeam.RemoveAt(0);
        UpdateUI();
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
            }
            else
            {
            currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
            RunTurn(index, ActionType.Attacking);
            }
        }
        else if (currentSnapshot.battleMode == BattleUIMode.ChoosingSwitchTeammate)
        {
            if (index == 3)
            {
                currentSnapshot.battleMode = BattleUIMode.ChoosingAction;
            }
            else
            {
            currentSnapshot.battleMode = BattleUIMode.ResolvingAction;
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
                }
            }
        }
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
                playerActiveShrimp.currentHP -= damage;
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
        }
    }
    private void KillPlayerShrimp()
    {
        playerActiveShrimp = playerTeam[0];
        playerTeam.RemoveAt(0);
    }
    private void KillEnemyShrimp()
    {
        enemyActiveShrimp = enemyTeam[0];
        enemyTeam.RemoveAt(0);
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
