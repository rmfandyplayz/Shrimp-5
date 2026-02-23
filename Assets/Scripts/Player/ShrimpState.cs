using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;

public class ShrimpState : MonoBehaviour
{
    public ShrimpDefinition definition;
    [HideInInspector] public int currentHP;
    [HideInInspector] public string instanceID;
    [HideInInspector] public List<AppliedStatus> statuses;
    
    public ShrimpState(ShrimpDefinition def)
    {
        definition = def;
        currentHP = def.maxHP;
    }
    void Start()
    {
        statuses = new List<AppliedStatus>();  
        currentHP = definition.maxHP;
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
                if (totalAttack < 1)
                {
                    totalAttack = 1;
                }
            }
        }
        return totalAttack;
    }

    public int GetSpeed()
    {
        int totalSpeed = definition.baseSpeed;
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
        if (totalSpeed < 1)
        {
            totalSpeed = 1;
        }
        return totalSpeed;
    }

    public void RecalculateHP(StatusDefinition status)
    {
        if (status.buffType == TypeOfBuff.percent)
        {
            currentHP = (int) (currentHP * status.valueChange);
        }
        else
        {
            currentHP = (int) (currentHP + status.valueChange);
        }
    }

    public List<int> UpdateStatuses()
    {
        List<int> removedList = new List<int>();
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            if (statuses[i].status.permanant == false)
            {
            statuses[i].remainingTurns--;

            if (statuses[i].remainingTurns <= 0)
            {
                statuses.RemoveAt(i);
                removedList.Add(i);
            }
            }
        }
        return removedList;
    }   
}
