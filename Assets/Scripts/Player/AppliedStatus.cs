using UnityEngine;

public class AppliedStatus
{
    public StatusDefinition status; 
    public int remainingTurns;

    public AppliedStatus(StatusDefinition s, int duration)
    {
        status = s;
        remainingTurns = duration;
    }

    public AppliedStatus(StatusDefinition statusDef)
    {
        status = statusDef;
        remainingTurns = statusDef.turnDuration;
    }
}
