using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public BattleController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ActivateAbility(ShrimpState shrimp, ShrimpState target)
    {
        if (shrimp.definition.ability.target == Target.Self)
        {
            AppliedStatus abilityEffect = new AppliedStatus(shrimp.definition.ability.effect, shrimp.definition.ability.effect.turnDuration);
            shrimp.statuses.Add(abilityEffect);
        }
        if (shrimp.definition.ability.target == Target.Opponent)
        {
            AppliedStatus abilityEffect = new AppliedStatus(shrimp.definition.ability.effect, shrimp.definition.ability.effect.turnDuration);
            target.statuses.Add(abilityEffect);
        }
    }
}
