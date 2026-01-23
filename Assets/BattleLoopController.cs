using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BattleLoopController : MonoBehaviour
{
    [Header("References")]
    public BattleController battleController;
    public GameObject battleCanvas;
    public GameObject selectionCanvas;
    
    [Header("Data")]
    public List<ShrimpDefinition> allPossibleShrimps; // Drag all your scriptable objects here
    public List<ShrimpState> playerTeam = new List<ShrimpState>();
    [HideInInspector] public int battleIndex;
    
    private void Start()
    {
        battleIndex = 0;
        ShowSelectionScreen();
    }

    // --- PHASE 1: SELECTION ---
    public void ShowSelectionScreen()
    {
        // Check cap
        if (playerTeam.Count >= 4)
        {
            Debug.Log("Team full, skipping selection.");
            StartBattle();
            return;
        }

        battleCanvas.SetActive(false);
        selectionCanvas.SetActive(true);
        
        // Generate 3 random options here and hook them up to buttons
        // (You'll need a simple UI script on the selection canvas to handle clicks)
    }

    // Call this when a button is clicked in the Selection UI
    public void OnShrimpSelected(ShrimpDefinition choice)
    {
        // Create a new runtime shrimp from the definition
        ShrimpState newShrimp = new ShrimpState(choice); 
        playerTeam.Add(newShrimp);
        StartBattle();
    }

    // --- PHASE 2: BATTLE ---
    public void StartBattle()
    {
        selectionCanvas.SetActive(false);
        battleCanvas.SetActive(true);
        
        // Pass the team to the battle controller
        // You'll need to update BattleController to accept a List<ShrimpState>
        battleController.StartNewBattle(playerTeam, OnBattleEnded, battleIndex);
    }

    // --- PHASE 3: CLEANUP ---
    private void OnBattleEnded(bool playerWon)
    {
        if (!playerWon)
        {
            // Game Over logic here
            Debug.Log("Game Over!");
            return;
        }

        // 1. Permadeath: Remove dead shrimps
        // RemoveAll is super efficient for this
        int removedCount = playerTeam.RemoveAll(shrimp => shrimp.GetHP() <= 0);
        if (removedCount > 0) Debug.Log($"RIP: {removedCount} shrimps died.");

        // 2. Heal survivors
        foreach (var shrimp in playerTeam)
        {
            shrimp.ResetState();
        }

        // 3. Loop back to selection
        ShowSelectionScreen();
    }
}