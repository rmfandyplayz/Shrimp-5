using UnityEngine;
public class AbilitySnapshot : MonoBehaviour
{
    public string displayName;  
    public Sprite icon;         
    public AbilityTrigger trigger; 

    public static AbilitySnapshot CreateAbilitytSnapshot(AbilityDefinition ability)
{
    return new AbilitySnapshot
    {
        displayName = ability.displayName,
        icon = ability.icon,
        trigger = ability.trigger
    };

}
}