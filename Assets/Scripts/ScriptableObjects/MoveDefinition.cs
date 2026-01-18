using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Move")]
public class MoveDefinition : ScriptableObject
{
    public string moveId;
    public string displayName;
    public bool hasEffect;
    public StatusDefinition effect;
    public Sprite icon;
    public int power;
                  
    public MoveTarget target;      

    [TextArea]
    public string description;
}
public enum MoveTarget
{
    Self, Opponent
}
