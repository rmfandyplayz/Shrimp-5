using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using Sh.UIContract;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BattleController : MonoBehaviour, IBattleCommands
{
    [Header("Refs")]
    public List<ShrimpState> playerTeam;
    public List<ShrimpState> enemyTeam;
    private System.Random rng;
    [HideInInspector] public ShrimpState playerActiveShrimp;
    [HideInInspector] public ShrimpState enemyActiveShrimp;
    [SerializeField] private TurnManager turnManager;
    private IBattleUI ui; 
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

    public void Back()
    {
        throw new NotImplementedException();
    }

    public void TogglePause()
    {
        throw new NotImplementedException();
    }

    public void DialogueSkipAll()
    {
        throw new NotImplementedException();
    }

    public void DialogueConfirm()
    {
        throw new NotImplementedException();
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
