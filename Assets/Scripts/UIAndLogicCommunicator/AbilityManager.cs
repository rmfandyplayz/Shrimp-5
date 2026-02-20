using Sh.UIContract;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public BattleController controller;
    public StatusManager statusManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ActivateAbility(ShrimpState shrimp, ShrimpState target)
    {
        BattleEvent abilityActivated = new();
        abilityActivated.eventType = BattleEventType.AbilityTriggered;
        abilityActivated.sourceId = shrimp.instanceID;
        abilityActivated.strings = new();
        abilityActivated.strings.Add(shrimp.definition.ability.abilityID);

        controller.ui.QueueEvent(abilityActivated);

        if (shrimp.definition.ability.target == Target.Self)
        {
            AppliedStatus abilityEffect = new AppliedStatus(shrimp.definition.ability.effect, shrimp.definition.ability.effect.turnDuration);
            statusManager.ApplyStatus(shrimp, abilityEffect);
        }
        if (shrimp.definition.ability.target == Target.Opponent)
        {
            AppliedStatus abilityEffect = new AppliedStatus(shrimp.definition.ability.effect, shrimp.definition.ability.effect.turnDuration);
            statusManager.ApplyStatus(target, abilityEffect);
        }
    }
}
