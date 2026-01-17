using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Status")]
public class StatusDefinition : ScriptableObject
{
    public string statusId;
    public string displayName;
    public string iconId;
    public StatAffected statChanged;
    public TypeOfAffect type;
    public int valueChange;
    public float percentChange;
    public bool permanant;
    public int turnDuration;
    [TextArea]
    public string description;
}
public enum StatAffected
{
    HP, Attack, Speed
}

public enum TypeOfAffect
{
    Positive, Negative
}
