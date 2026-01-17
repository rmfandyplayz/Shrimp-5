using UnityEngine;
public class StatusSnapshot : MonoBehaviour
{
    public string displayName;  
    public Sprite icon;         
    public int remainingTurns;  
    public TypeOfAffect type;
    public static StatusSnapshot CreateStatusSnapshot(AppliedStatus appliedStatus)
{
    return new StatusSnapshot
    {
        displayName = appliedStatus.status.displayName,
        icon = appliedStatus.status.icon,  // or look up by iconId
        remainingTurns = appliedStatus.remainingTurns,
        type = appliedStatus.status.affectType
    };
}     
}