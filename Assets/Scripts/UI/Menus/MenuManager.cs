using UnityEngine;
using UnityEngine.EventSystems;

// written by andy
// helper script for the main menu to make certain things less of a pain in the ass
public class MenuManager : MonoBehaviour
{
    [Header("script references")]
    [SerializeField] MenuBase mainMenu;
    [SerializeField] MenuBase settingsMenu;
    [SerializeField] MenuBase creditsChoiceMenu; // screen that prompts devs & audio button
    [SerializeField] MenuBase creditsDevsMenu; // dev list
    [SerializeField] MenuBase creditsAudioMenu; // music list

    private MenuBase currentMenu;

    public static MenuManager Instance; // guess we doin singletons now

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        currentMenu = mainMenu;

        settingsMenu.gameObject.SetActive(false);
        //creditsChoiceMenu.gameObject.SetActive(false);
        //creditsDevsMenu.gameObject.SetActive(false);
        //creditsAudioMenu.gameObject.SetActive(false);
         
        // animate the current menu aka the main menu when game first loads in
        currentMenu.GetCanvasGroup().interactable = false;
        currentMenu.AnimateIn(() =>
        {
            currentMenu.GetCanvasGroup().interactable = true;
            EventSystem.current.SetSelectedGameObject(null); // well apparently i gotta do this thing lol
            EventSystem.current.SetSelectedGameObject(currentMenu.GetFirstSelected());
        });
    }

    public void SwitchMenu(MenuBase nextMenu)
    {
        if (currentMenu == nextMenu)
            return;

        // lock everything
        if(currentMenu != null)
            currentMenu.GetCanvasGroup().interactable = false;
        nextMenu.GetCanvasGroup().interactable = false;

        // close old and open new
        currentMenu.AnimateOut(() =>
        {
            currentMenu.gameObject.SetActive(false);
            nextMenu.gameObject.SetActive(true);
            nextMenu.AnimateIn(() =>
            {
                // unlock controls and select first
                nextMenu.GetCanvasGroup().interactable = true;
                nextMenu.GetCanvasGroup().blocksRaycasts = true;

                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(nextMenu.GetFirstSelected());

                currentMenu = nextMenu;
            });
        });
    }

    
}
