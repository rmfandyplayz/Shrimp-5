using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class BattleLoopController : MonoBehaviour
{
    [Header("References")]
    public BattleController battleController;
    public List<GameObject> battleCanvas;
    public GameObject selectionCanvas;
    
    [Header("Data")]
    public List<ShrimpDefinition> allPossibleShrimps; // Drag all your scriptable objects here
    public List<ShrimpState> playerTeam = new List<ShrimpState>();
    [HideInInspector] public int battleIndex;
    public ShrimpSelectionUI selectionUIScript;
    
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

        foreach(GameObject canvas in battleCanvas)
        {
            canvas.SetActive(false);
        }
        selectionCanvas.SetActive(true);
        
        // Generate 3 random options here and hook them up to buttons
        // 2. Logic to pick 3 UNIQUE random shrimps
        List<ShrimpDefinition> randomOptions = new List<ShrimpDefinition>();
        List<ShrimpDefinition> pool = new List<ShrimpDefinition>(allPossibleShrimps); // Copy master list so we can remove items

        int choicesToOffer = 3;

        // Safety check: if we have fewer than 3 total shrimps defined, just show what we have
        if (pool.Count < choicesToOffer)
        {
            choicesToOffer = pool.Count;
        }

        for (int i = 0; i < choicesToOffer; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            
            // Add to our options
            randomOptions.Add(pool[randomIndex]);
            
            // Remove from the temporary pool so we don't pick it again
            pool.RemoveAt(randomIndex);
        }

        // 3. Send the list to the UI script to update the buttons
        selectionUIScript.OpenSelectionScreen(randomOptions);
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
        foreach(GameObject canvas in battleCanvas)
        {
            canvas.SetActive(true);
        }
        
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
            // TODO: load title screen

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