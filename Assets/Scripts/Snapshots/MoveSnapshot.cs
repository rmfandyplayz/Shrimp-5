using UnityEngine;
public class MoveSnapshot : MonoBehaviour
{
    public string displayName;       
    public Sprite icon;              
    public string description;  
    public int power;                
    public MoveTarget target;       

    public static MoveSnapshot CreateMoveSnapshot(MoveDefinition move)
{
    return new MoveSnapshot
    {
        displayName = move.displayName,
        icon = move.icon,
        description = move.description,
        power = move.power,
        target = move.target
    };
} 
}
