using Sh.UIContract;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public BattleController controller;
    public StatusManager statusManager;
    public AbilityManager abilityManager;
    
    public void RunAttack(ShrimpState attacker, ShrimpState target, MoveDefinition move)
    {
        BattleEvent attack = new();
        attack.eventType = BattleEventType.Attack;
        attack.sourceId = attacker.instanceID;
        attack.moveId = move.moveID;
        controller.ui.QueueEvent(attack);

        int damage = attacker.GetAttack()*move.power;
        if (move.target == MoveTarget.Self)
        {
            attacker.currentHP = attacker.GetHP() - damage;
            if(damage > 0 && (attacker.definition.ability.trigger == AbilityTrigger.OnDamaged || attacker.definition.ability.trigger == AbilityTrigger.OnAttack))
            {
                BattleEvent damageSelf = new();
                damageSelf.sourceId = attacker.instanceID;
                damageSelf.eventType = BattleEventType.TakeDamage;
                damageSelf.finalValue = attacker.currentHP;
                damageSelf.deltaValue = damage;
                controller.ui.QueueEvent(damageSelf);

                abilityManager.ActivateAbility(attacker, target);
            }

            if (move.hasEffect)
            {
                AppliedStatus status = new AppliedStatus(move.effect, move.effect.turnDuration);
                statusManager.ApplyStatus(attacker, status); 
            }
        }
        else
        {
            target.currentHP = target.GetHP() - damage;
            if (damage > 0)
            {
                BattleEvent damageOpponent = new();
                damageOpponent.sourceId = target.instanceID;
                damageOpponent.eventType = BattleEventType.TakeDamage;
                damageOpponent.finalValue = target.currentHP;
                damageOpponent.deltaValue = damage;
                controller.ui.QueueEvent(damageOpponent);

                if (attacker.definition.ability.trigger == AbilityTrigger.OnAttack)
                {
                    abilityManager.ActivateAbility(attacker, target);
                }
                if (target.definition.ability.trigger == AbilityTrigger.OnDamaged)
                {
                    abilityManager.ActivateAbility(target, attacker);
                }
            }
            if (move.hasEffect)
            {
                AppliedStatus status = new AppliedStatus(move.effect, move.effect.turnDuration);
                statusManager.ApplyStatus(target, status);
            }
        }
        
    }
    public MoveDefinition ChooseEnemyMove(MoveDefinition playerMove)
    {
        int enemyAttack =  controller.enemyActiveShrimp.GetAttack();
        int playerAttack = controller.playerActiveShrimp.GetAttack();
        MoveDefinition[] enemyMoves = controller.enemyActiveShrimp.definition.moves;
        int[] moveScores = new int[3];
        int highestScoreIndex = 0;
        MoveDefinition highestScoreMove = enemyMoves[0];
        for (int i = 0; i <= 2; i++)
        {
            if (enemyMoves[i].power*enemyAttack >= controller.playerActiveShrimp.currentHP)
            {
                moveScores[i] += 20;
            }
            if (enemyMoves[i].power*enemyAttack >= 0)
            {
                moveScores[i] += 5;
            }
            if ((playerMove != null && (playerMove.power*playerAttack >= controller.enemyActiveShrimp.currentHP)) && ((enemyMoves[i].hasEffect && (enemyMoves[i].effect.effectType == TypeOfEffect.Positive) && (enemyMoves[i].target == MoveTarget.Self) && (enemyMoves[0].effect.statChanged == StatAffected.HP)) || (enemyMoves[i].hasEffect && (enemyMoves[i].effect.effectType == TypeOfEffect.Negative) && (enemyMoves[i].target == MoveTarget.Opponent) && (enemyMoves[i].effect.statChanged == StatAffected.Attack))))
            {
                moveScores[i] += 6;
            }
            if ((playerMove != null && (playerMove.power*playerAttack < controller.enemyActiveShrimp.currentHP/2)) && (((enemyMoves[i].hasEffect) && (enemyMoves[i].effect.statChanged == StatAffected.Attack)) || ((enemyMoves[i].hasEffect) && (enemyMoves[i].effect.statChanged == StatAffected.Speed) && (controller.enemyActiveShrimp.GetSpeed() <= controller.playerActiveShrimp.GetSpeed()))))
            {
                moveScores[i] += 6;
            }
            if (moveScores[i] > moveScores[highestScoreIndex])
            {
                highestScoreIndex = i;
                highestScoreMove = enemyMoves[i];
            }
        }
        
        return highestScoreMove;
    }
}
