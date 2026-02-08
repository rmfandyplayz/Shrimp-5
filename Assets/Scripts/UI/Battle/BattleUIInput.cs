using Sh.UIContract;
using UnityEngine;

// written by andy
// handles input and
// communicates with battle logic about what the player wants to do
public class BattleUIInput : MonoBehaviour
{
    private IBattleCommands battleLogic;

    private void Awake()
    {
        battleLogic = FindFirstObjectByType<BattleController>() as IBattleCommands;
    }


}
