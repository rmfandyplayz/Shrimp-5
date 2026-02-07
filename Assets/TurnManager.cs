using System;
using UnityEditor;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public BattleController controller;
    public AttackManager attackManager;
    public StatusManager statusManager;
    public DeathManager deathManager;
    public AbilityManager abilityManager;
    public void RunTurn(string playerActionID, ActionType action)
    {
        int playerActionIndex = 0;
        if (controller.playerActiveShrimp.definition.ability.trigger == AbilityTrigger.OnTurnStart)
        {
            abilityManager.ActivateAbility(controller.playerActiveShrimp, controller.enemyActiveShrimp);
        }
        if (controller.enemyActiveShrimp.definition.ability.trigger == AbilityTrigger.OnTurnStart)
        {
            abilityManager.ActivateAbility(controller.enemyActiveShrimp, controller.playerActiveShrimp);
        }
        int playerSpeed = controller.playerActiveShrimp.GetSpeed();
        int enemySpeed = controller.enemyActiveShrimp.GetSpeed();
        if (action == ActionType.Switching)
        {
            ShrimpState temp = controller.playerActiveShrimp;
            controller.playerActiveShrimp = controller.playerTeam[playerActionIndex];
            controller.playerTeam[playerActionIndex] = temp;
            if (controller.playerActiveShrimp.definition.ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                abilityManager.ActivateAbility(controller.playerActiveShrimp, controller.enemyActiveShrimp);
            }
            attackManager.RunAttack(controller.enemyActiveShrimp, controller.playerActiveShrimp, attackManager.ChooseEnemyMove(playerActionIndex));
        }
        else
        {
            if (controller.playerActiveShrimp.GetSpeed() == controller.enemyActiveShrimp.GetSpeed())
            {
                System.Random rng = new System.Random();
                int whoseTurn = rng.Next(0,2);
                if (whoseTurn == 0) {playerSpeed++;}
                if (whoseTurn == 0) {enemySpeed++;}
            }
            if (playerSpeed > enemySpeed)
            {
                attackManager.RunAttack(controller.playerActiveShrimp, controller.enemyActiveShrimp, playerActionIndex);
                deathManager.CheckForDeath();
                attackManager.RunAttack(controller.enemyActiveShrimp, controller.playerActiveShrimp, attackManager.ChooseEnemyMove(playerActionIndex));
            }
            else
            {
                attackManager.RunAttack(controller.enemyActiveShrimp, controller.playerActiveShrimp, attackManager.ChooseEnemyMove(playerActionIndex));
                deathManager.CheckForDeath();
                attackManager.RunAttack(controller.playerActiveShrimp, controller.enemyActiveShrimp, playerActionIndex);
            }
        }
        deathManager.CheckForDeath();
        if (controller.playerActiveShrimp.definition.ability.trigger == AbilityTrigger.OnTurnEnd)
        {
            abilityManager.ActivateAbility(controller.playerActiveShrimp, controller.enemyActiveShrimp);
        }
        if (controller.enemyActiveShrimp.definition.ability.trigger == AbilityTrigger.OnTurnEnd)
        {
            abilityManager.ActivateAbility(controller.enemyActiveShrimp, controller.playerActiveShrimp);
        }
        statusManager.DecreaseStatusTurnsLeft(controller.playerActiveShrimp);
        statusManager.DecreaseStatusTurnsLeft(controller.enemyActiveShrimp);

    }
}
