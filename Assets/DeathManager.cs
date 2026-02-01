using DG.Tweening.Core;
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
            Die();
        }
        if (controller.playerActiveShrimp.currentHP <= 0)
        {
            if (controller.playerActiveShrimp.definition.ability.trigger == AbilityTrigger.OnDeath)
            {
                abilityManager.ActivateAbility(controller.playerActiveShrimp, controller.enemyActiveShrimp);
            }
            controller.playerActiveShrimp = controller.playerTeam[0];
            controller.playerTeam.RemoveAt(0);
            if (controller.playerActiveShrimp.definition.ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                abilityManager.ActivateAbility(controller.playerActiveShrimp, controller.enemyActiveShrimp);
            }
        }
        if (controller.enemyTeam.Count ==0)
        {
            Die();
        }
        if (controller.enemyActiveShrimp.currentHP <= 0)
        {
            if (controller.enemyActiveShrimp.definition.ability.trigger == AbilityTrigger.OnDeath)
            {
                abilityManager.ActivateAbility(controller.enemyActiveShrimp, controller.playerActiveShrimp);
            }
            controller.enemyActiveShrimp = controller.enemyTeam[0];
            controller.enemyTeam.RemoveAt(0);
            if (controller.enemyActiveShrimp.definition.ability.trigger == AbilityTrigger.OnSwitchIn)
            {
                abilityManager.ActivateAbility(controller.enemyActiveShrimp, controller.playerActiveShrimp);
            }
        }
    }
    private void Die()
    {
        
    }
}
