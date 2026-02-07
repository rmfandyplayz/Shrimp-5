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
    [SerializeField] private BattleUIModel uiModel;
    public List<ShrimpState> playerTeam;
    public List<ShrimpState> enemyTeam;
    private System.Random rng;
    [HideInInspector] public ShrimpState playerActiveShrimp;
    [HideInInspector] public ShrimpState enemyActiveShrimp;
    [SerializeField] private TurnManager turnManager;
    private ActionType action;
    private IBattleUI ui; 
    [SerializeField] private MonoBehaviour uiScript;

    void Awake()
    {
        ui = uiScript as IBattleUI;
    }

    void Start() 
    {
        // example usage
        ui.InitializeBattle();
        ui.QueueEvent();
    }

    public void SelectMove(string moveID)
    {
        turnManager.RunTurn(moveID, action);
    }

    public void Back()
    {
        throw new NotImplementedException();
    }

    public void TogglePause()
    {
        throw new NotImplementedException();
    }

    public void DialogueSkip()
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
}

public enum ActionType
{
    Switching, Attacking
}
public enum User
{
    Player, Enemy
}
