using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ShrimpSelectionUI : MonoBehaviour
{
    [Header("Dependencies")]
    public BattleLoopController gameManager; // Drag the manager here
    public List<GameObject> battleUIObject;   // Drag the "GameUI" or "BattleCanvas" here
    [SerializeField] GameObject firstSelected;

    [Header("Button Components")]
    public Button[] choiceButtons;      // Drag your 3 buttons here
    public TextMeshProUGUI[] nameTexts; // Drag the text component of each button here
    public Image[] portraitImages;      // Drag the image component of each button here

    private List<ShrimpDefinition> currentOptions;

    // Called by GameLoopManager
    public void OpenSelectionScreen(List<ShrimpDefinition> options)
    {
        currentOptions = options;

        // 1. Hide Battle UI, Show Selection
        foreach (GameObject canvas in battleUIObject)
        {
        canvas.SetActive(false);
        }
        gameObject.SetActive(true);

        // 2. Setup Buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < options.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);

                // Setup visuals
                nameTexts[i].text = options[i].displayName;
                portraitImages[i].sprite = SpriteResolver.Get(options[i].shrimpSpriteID); // Or whatever your variable is called

                // Setup click listener (classic closure fix)
                int index = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnButtonClicked(index));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    void OnButtonClicked(int index)
    {
        // 1. Tell manager what we picked
        gameManager.OnShrimpSelected(currentOptions[index]);

        // 2. Hide self
        gameObject.SetActive(false);
        foreach (GameObject canvas in battleUIObject)
        {
        canvas.SetActive(true);
        }
    }
}