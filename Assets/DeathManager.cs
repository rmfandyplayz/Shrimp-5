using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public BattleController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void CheckForDeath()
    {
        if (controller.playerTeam.Count ==0)
        {
            Die();
        }
        if (controller.playerActiveShrimp.currentHP <= 0)
        {
            controller.playerActiveShrimp = controller.playerTeam[0];
            controller.playerTeam.RemoveAt(0);
        }
        if (controller.enemyTeam.Count ==0)
        {
            Die();
        }
        if (controller.enemyActiveShrimp.currentHP <= 0)
        {
            controller.enemyActiveShrimp = controller.enemyTeam[0];
            controller.enemyTeam.RemoveAt(0);
        }
    }
    private void Die()
    {
        
    }
}
