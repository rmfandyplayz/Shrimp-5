using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Battle/Ability")]
public class AbilityDefinition : ScriptableObject
{
    public string displayName;

    public string iconID;

    public AbilityTrigger trigger; 
    public StatusDefinition effect;               

    [TextArea]
    public string description;
    public Target target;
}

public enum AbilityTrigger
{
    OnAttack,
    OnDamaged,
    OnSwitchIn,
    OnTurnStart,
    OnTurnEnd,
    OnDeath
}
public enum Target
{
    Self, Opponent
}