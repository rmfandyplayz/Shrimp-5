using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Status")]
public class StatusDefinition : ScriptableObject
{
    public string statusId;
    public string displayName;
    public Sprite icon;
    public StatAffected statChanged;
    public TypeOfAffect affectType;
    public TypeOfBuff buffType;
    public double valueChange;
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

public enum TypeOfBuff
{
    percent, value
}
