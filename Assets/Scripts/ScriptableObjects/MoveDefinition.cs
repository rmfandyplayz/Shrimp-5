using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Move")]
public class MoveDefinition : ScriptableObject
{
    public string moveId;
    public string displayName;

    public string iconId;
    public int power;              
    public MoveTarget target;      

    [TextArea]
    public string description;
}
public enum MoveTarget
{
    Self, Opponent
}
