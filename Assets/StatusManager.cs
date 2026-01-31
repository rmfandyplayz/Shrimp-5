using UnityEngine;

public class StatusManager : MonoBehaviour
{
    
    public void ApplyStatus(ShrimpState shrimp, AppliedStatus status)
    {
        shrimp.statuses.Add(status);
    }

    public void DecreaseStatusTurnsLeft(ShrimpState shrimp)
    {
        foreach (AppliedStatus status in shrimp.statuses)
        {
            if (!status.status.permanant)
            {
                status.remainingTurns--;
            }
            if (status.remainingTurns == 0)
            {
                shrimp.statuses.Remove(status);
            }
        }
        

    }
}
