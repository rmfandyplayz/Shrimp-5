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
            ShrimpState temp = playerTeam[0];
            playerTeam[0] = playerTeam[index + 1];
            playerTeam[index + 1] = temp;
            EnemyAttack(EnemyMoveSelection(index));
        }
        else
        {
        int playerSpeed = playerTeam[0].GetSpeed();
        int enemySpeed = enemyTeam[0].GetSpeed();
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
        if (playerTeam[0].currentHP <= 0)
        {
            KillPlayerShrimp();
        }
        else
        {
            MoveDefinition move = playerTeam[0].definition.moves[index];
            int shrimpPower = playerTeam[0].GetAttack();
            int damage = shrimpPower*move.power;
            if (move.target == MoveTarget.Opponent)
            {
                enemyTeam[0].currentHP -= damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    enemyTeam[0].statuses.Add(newStatus);
                }
            }
            else
            {
                playerTeam[0].currentHP -= damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    playerTeam[0].statuses.Add(newStatus);
                }
            }
        }
    }

    private int EnemyMoveSelection(int playerAttackIndex)
    {
        int enemyAttack = enemyTeam[0].GetAttack();
        int playerAttack = playerTeam[0].GetAttack();
        MoveDefinition[] enemyMoves = enemyTeam[0].definition.moves;
        MoveDefinition playerMove = playerTeam[0].definition.moves[playerAttackIndex];
        int[] moveScores = new int[3];
        int highestScoreIndex = 0;
        for (int i = 0; i <= 2; i++)
        {
            if (enemyMoves[i].power*enemyAttack >= playerTeam[0].currentHP)
            {
                moveScores[i] += 20;
            }
            if (enemyMoves[i].power*enemyAttack >= 0)
            {
                moveScores[i] += 5;
            }
            if ((playerMove.power*playerAttack >= enemyTeam[0].currentHP) && ((enemyMoves[i].hasEffect && (enemyMoves[i].effect.effectType == TypeOfEffect.Positive) && (enemyMoves[i].target == MoveTarget.Self) && (enemyMoves[0].effect.statChanged == StatAffected.HP)) || (enemyMoves[i].hasEffect && (enemyMoves[i].effect.effectType == TypeOfEffect.Negative) && (enemyMoves[i].target == MoveTarget.Opponent) && (enemyMoves[i].effect.statChanged == StatAffected.Attack))))
            {
                moveScores[i] += 6;
            }
            if ((playerMove.power*playerAttack < enemyTeam[0].currentHP/2) && (((enemyMoves[i].hasEffect) && (enemyMoves[i].effect.statChanged == StatAffected.Attack)) || ((enemyMoves[i].hasEffect) && (enemyMoves[i].effect.statChanged == StatAffected.Speed) && (enemyTeam[0].GetSpeed() <= playerTeam[0].GetSpeed()))))
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
        if (enemyTeam[0].currentHP <= 0)
        {
            KillEnemyShrimp();
        }
        else
        {
            MoveDefinition move = enemyTeam[0].definition.moves[index];
            int shrimpPower = enemyTeam[0].GetAttack();
            int damage = shrimpPower*move.power;
            if (move.target == MoveTarget.Opponent)
            {
                playerTeam[0].currentHP -= damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    playerTeam[0].statuses.Add(newStatus);
                }
            }
            else
            {
                enemyTeam[0].currentHP -= damage;
                if (move.hasEffect)
                {
                    AppliedStatus newStatus = new AppliedStatus(move.effect, move.effect.turnDuration);
                    enemyTeam[0].statuses.Add(newStatus);
                }
            }
        }
    }
    private void KillPlayerShrimp()
    {
        
    }
    private void KillEnemyShrimp()
    {
        
    }

    public void DialogueConfirm()
    {
        throw new NotImplementedException();
    }

    public void DialogueSkipAll()
    {
        throw new NotImplementedException();
    }
}

public enum ActionType
{
    Switching, Attacking
}
