using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Move")]
public class MoveDefinition : ScriptableObject
{
    public string displayName;
    public bool hasEffect;
    public StatusDefinition effect;
    public string iconID;
    public int power;
    public string moveID;     
    public MoveTarget target;      

    [TextArea]
    public string shortDescription;
    public string longDescription;
}
public enum MoveTarget
{
    Self, Opponent
}
