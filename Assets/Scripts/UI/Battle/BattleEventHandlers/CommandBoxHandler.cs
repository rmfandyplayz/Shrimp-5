using Sh.UIContract;
using System.Collections;
using UnityEngine;

// written by andy
// handles all necessary changes to the command box
public class CommandBoxHandler : MonoBehaviour, IBattleEventHandler
{
    [SerializeField] CommandBox commandBox;


    public bool CanHandle(BattleEventType eventType)
    {
        throw new System.NotImplementedException();
    }

    public void ForceSkip()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator HandleEvent(BattleEvent evt)
    {
        throw new System.NotImplementedException();
    }
}
