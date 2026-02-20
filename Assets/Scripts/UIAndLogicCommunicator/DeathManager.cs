using Sh.UIContract;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public BattleController controller;
    public AbilityManager abilityManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void CheckForDeath()
    {
        if (controller.playerTeam.Count ==0)
        {
            Die(User.Player);
        }
        if (controller.playerActiveShrimp.currentHP <= 0)
        {
            BattleEvent playerDied = new BattleEvent();
            playerDied.eventType = BattleEventType.CharacterDied;
            playerDied.sourceId = controller.playerActiveShrimp.instanceID;
            controller.ui.QueueEvent(playerDied);

            if (controller.playerActiveShrimp.definition.ability.trigger == AbilityTrigger.OnDeath)
            {
                abilityManager.ActivateAbility(controller.playerActiveShrimp, controller.enemyActiveShrimp);
            }

            controller.playerActiveShrimp = controller.playerTeam[0];
            controller.playerTeam.RemoveAt(0);

            BattleEvent playerSwitchIn = new BattleEvent();
            playerSwitchIn.eventType = BattleEventType.SwitchingShrimp;
            playerSwitchIn.sourceId = controller.playerActiveShrimp.instanceID;
            controller.ui.QueueEvent(playerSwitchIn);

            if (controller.playerActiveShrimp.definition.ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                abilityManager.ActivateAbility(controller.playerActiveShrimp, controller.enemyActiveShrimp);
            }
        }
        if (controller.enemyTeam.Count ==0)
        {
            Die(User.Enemy);
        }
        if (controller.enemyActiveShrimp.currentHP <= 0)
        {
            BattleEvent enemyDied = new BattleEvent();
            enemyDied.eventType = BattleEventType.CharacterDied;
            enemyDied.sourceId = controller.enemyActiveShrimp.instanceID;
            controller.ui.QueueEvent(enemyDied);

            if (controller.enemyActiveShrimp.definition.ability.trigger == AbilityTrigger.OnDeath)
            {
                abilityManager.ActivateAbility(controller.enemyActiveShrimp, controller.playerActiveShrimp);
            }
            controller.enemyActiveShrimp = controller.enemyTeam[0];
            controller.enemyTeam.RemoveAt(0);

            BattleEvent enemySwitchIn = new BattleEvent();
            enemySwitchIn.eventType = BattleEventType.SwitchingShrimp;
            enemySwitchIn.sourceId = controller.enemyActiveShrimp.instanceID;
            controller.ui.QueueEvent(enemySwitchIn);

            if (controller.enemyActiveShrimp.definition.ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                abilityManager.ActivateAbility(controller.enemyActiveShrimp, controller.playerActiveShrimp);
            }
        }
    }
    private void Die(User user)
    {
        if (user == User.Player)
        {
            BattleEvent battleWon = new BattleEvent();
            battleWon.eventType = BattleEventType.BattleWon;
            controller.ui.QueueEvent(battleWon);
        }
        else
        {
            BattleEvent battleLost = new BattleEvent();
            battleLost.eventType = BattleEventType.BattleLost;
            controller.ui.QueueEvent(battleLost);
        }
    }
}
