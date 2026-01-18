using UnityEngine;

public class AppliedStatus : MonoBehaviour
{
    public StatusDefinition status; 
    public int remainingTurns;

    public AppliedStatus()
    {
    }

    public AppliedStatus(StatusDefinition statusDef)
    {
        status = statusDef;
        remainingTurns = statusDef.turnDuration;
    }
}
