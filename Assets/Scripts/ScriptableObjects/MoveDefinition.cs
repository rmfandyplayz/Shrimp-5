using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Move")]
public class MoveDefinition : ScriptableObject
{
    public string displayName;
    public bool hasEffect;
    public StatusDefinition effect;
    public string iconID;
    public int power;
                  
    public MoveTarget target;      

    [TextArea]
    public string description;
}
public enum MoveTarget
{
    Self, Opponent
}
