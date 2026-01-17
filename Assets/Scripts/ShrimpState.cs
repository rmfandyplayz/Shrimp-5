using System.Collections.Generic;
using UnityEngine;

public class ShrimpState : MonoBehaviour
{
    public ShrimpDefinition definition;
    public int currentHP;
    public List<AppliedStatus> statuses = new List<AppliedStatus>();
    public ShrimpState(ShrimpDefinition def)
    {
        definition = def;
        currentHP = def.maxHP;
    }

    public int GetAttack()
    {
        int totalAttack = definition.baseAttack;
        foreach (AppliedStatus status in statuses)
        {
            if(status.status.statChanged == StatAffected.Attack)
            {
                if (status.status.buffType == TypeOfBuff.percent)
                {
                totalAttack = (int) (totalAttack*status.status.valueChange);
                }
                else if (status.status.buffType == TypeOfBuff.value)
                {
                totalAttack += (int) status.status.valueChange;
                }
            }
        }
        return totalAttack;
    }

    public int GetSpeed()
    {
        int totalSpeed = definition.baseAttack;
        foreach (AppliedStatus status in statuses)
        {
            if(status.status.statChanged == StatAffected.Speed)
            {
                if (status.status.buffType == TypeOfBuff.percent)
                {
                totalSpeed = (int) (totalSpeed*status.status.valueChange);
                }
                else if (status.status.buffType == TypeOfBuff.value)
                {
                totalSpeed += (int) status.status.valueChange;
                }
            }
        }
        return totalSpeed;
    }

    public int GetHP()
    {
        int totalHP = definition.baseAttack;
        foreach (AppliedStatus status in statuses)
        {
            if(status.status.statChanged == StatAffected.HP)
            {
                if (status.status.buffType == TypeOfBuff.percent)
                {
                totalHP = (int) (totalHP*status.status.valueChange);
                }
                else if (status.status.buffType == TypeOfBuff.value)
                {
                totalHP += (int) status.status.valueChange;
                }
            }
        }
        return totalHP;
    }

    public void UpdateStatuses()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            if (statuses[i].status.permanant == false)
            {
            statuses[i].remainingTurns--;

            if (statuses[i].remainingTurns <= 0)
            {
                statuses.RemoveAt(i);
            }
            }
        }
    }   
}
