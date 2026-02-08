using System.Collections.Generic;
using Sh.UIContract;
using UnityEngine;

public class BattleController : MonoBehaviour, IBattleCommands
{
    [Header("Refs")]
    public List<ShrimpState> playerTeam;
    public List<ShrimpState> enemyTeam;
    private System.Random rng;
    [HideInInspector] public ShrimpState playerActiveShrimp;
    [HideInInspector] public ShrimpState enemyActiveShrimp;
    [SerializeField] private TurnManager turnManager;
    [HideInInspector] public IBattleUI ui; 
    [SerializeField] private MonoBehaviour uiScript;

    void Awake()
    {
        ui = uiScript as IBattleUI;
    }

    void Start() 
    {
        // example usage
        // ui.InitializeBattle();
        // ui.QueueEvent();
    }

    public void TogglePause()
    {
        
    }

    public void SelectAction(string actionID, ActionType actionType)
    {
        turnManager.RunTurn(actionID, actionType);
    }
}

public enum ActionType
{
    Switching, Attacking
}
public enum User
{
    Player, Enemy
}
