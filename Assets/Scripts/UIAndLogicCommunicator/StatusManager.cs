using Sh.UIContract;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    [SerializeField] private BattleController controller;
    
    public void ApplyStatus(ShrimpState shrimp, AppliedStatus status)
    {
        shrimp.statuses.Add(status);
        if (status.status.statChanged == StatAffected.HP)
        {
                if(status.status.valueChange > 0)
                {
                    BattleEvent healed = new();
                    healed.eventType = BattleEventType.Heal;
                    healed.sourceId = shrimp.definition.shrimpID;
                    healed.deltaValue = (int) status.status.valueChange;
                    healed.finalValue = shrimp.GetHP();

                    controller.ui.QueueEvent(healed);
                }
                else
                {
                    BattleEvent damaged = new();
                    damaged.eventType = BattleEventType.TakeDamage;
                    damaged.sourceId = shrimp.definition.shrimpID;
                    damaged.deltaValue = (int) -status.status.valueChange;
                    damaged.finalValue = shrimp.GetHP();
                    controller.ui.QueueEvent(damaged);
                }
        }
        else
        {
        BattleEvent applyStatus = new();
        applyStatus.strings = new();
        applyStatus.strings.Add(status.status.statusID);
        applyStatus.eventType = BattleEventType.StatusApplied;
        applyStatus.sourceId = shrimp.definition.shrimpID;
        controller.ui.QueueEvent(applyStatus);
        }
    }

    public void DecreaseStatusTurnsLeft(ShrimpState shrimp)
    {
        foreach (AppliedStatus status in shrimp.statuses)
        {
            if (!status.status.permanant)
            {
                status.remainingTurns--;
                if (status.remainingTurns == 0)
                {
                    BattleEvent statusGone = new();
                    statusGone.eventType = BattleEventType.StatusRemoved;
                    statusGone.strings = new();
                    statusGone.strings.Add(status.status.statusID);
                    controller.ui.QueueEvent(statusGone);
                    
                    shrimp.statuses.Remove(status);
                }
            }
        }
        

    }
}
