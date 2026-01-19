using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Battle/Ability")]
public class AbilityDefinition : ScriptableObject
{
    public string abilityId;
    public string displayName;

    public string iconID;

    public AbilityTrigger trigger; 
    public StatusDefinition effect;               

    [TextArea]
    public string description;
}

public enum AbilityTrigger
{
    OnAttack,
    OnDamaged,
    OnSwitchIn,
    OnTurnStart,
    OnTurnEnd,
}