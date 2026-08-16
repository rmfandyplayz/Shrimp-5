using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// written by andy
// hardened + menu lifecycle hooks by Claude Opus 5
// helper script for the main menu to make certain things less of a pain in the ass
public class MenuManager : MonoBehaviour
{
    [Header("script references")]
    [SerializeField] MenuBase mainMenu;
    [SerializeField] MenuBase settingsMenu;
    [SerializeField] MenuBase creditsMenu;

    private MenuBase currentMenu;

    // true while a menu is animating in or out. without this, mashing submit during a
    // transition starts a second SwitchMenu on top of the first and the two fight over
    // which menu ends up active
    private bool isTransitioning;

    public static MenuManager Instance; // guess we doin singletons now

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            // was Destroy(this), which killed the component but left the GameObject sitting
            // in the scene
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        currentMenu = mainMenu;

        settingsMenu.gameObject.SetActive(false);
        creditsMenu.gameObject.SetActive(false);

        // animate the current menu aka the main menu when game first loads in
        currentMenu.GetCanvasGroup().interactable = false;
        currentMenu.AnimateIn(() =>
        {
            currentMenu.GetCanvasGroup().interactable = true;
            EventSystem.current.SetSelectedGameObject(null); // well apparently i gotta do this thing lol
            EventSystem.current.SetSelectedGameObject(currentMenu.GetFirstSelected());

            currentMenu.OnMenuOpened();
        });
    }

    /// <summary>
    /// Animates the current menu out and the next one in, moving the cursor onto the new
    /// menu's first selectable once it's done.
    ///
    /// Ignores calls made while a transition is already running.
    /// </summary>
    public void SwitchMenu(MenuBase nextMenu)
    {
        if (nextMenu == null || currentMenu == nextMenu || isTransitioning)
            return;

        isTransitioning = true;

        // lock everything
        if (currentMenu != null)
            currentMenu.GetCanvasGroup().interactable = false;
        nextMenu.GetCanvasGroup().interactable = false;

        // nothing to animate out on the very first switch
        if (currentMenu == null)
        {
            OpenMenu(nextMenu);
            return;
        }

        currentMenu.OnMenuClosed();

        MenuBase closingMenu = currentMenu;

        // close old and open new
        closingMenu.AnimateOut(() =>
        {
            closingMenu.gameObject.SetActive(false);
            OpenMenu(nextMenu);
        });
    }

    // second half of SwitchMenu, split out so the "no current menu" path can reuse it
    private void OpenMenu(MenuBase nextMenu)
    {
        nextMenu.gameObject.SetActive(true);
        nextMenu.AnimateIn(() =>
        {
            // unlock controls and select first
            nextMenu.GetCanvasGroup().interactable = true;
            nextMenu.GetCanvasGroup().blocksRaycasts = true;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(nextMenu.GetFirstSelected());

            currentMenu = nextMenu;
            isTransitioning = false;

            currentMenu.OnMenuOpened();
        });
    }

    public void PlayGame(string sceneName)
    {
        if (currentMenu == null)
        {
            SceneLoader.Load(sceneName);
            return;
        }

        currentMenu.GetCanvasGroup().interactable = false;
        currentMenu.OnMenuClosed();

        currentMenu.AnimateOut(() =>
        {
            SceneLoader.Load(sceneName);
        });
    }

    public void QuitGame()
    {
        if (currentMenu != null)
        {
            currentMenu.GetCanvasGroup().interactable = false;
            currentMenu.OnMenuClosed();
            currentMenu.gameObject.SetActive(false);
        }

        StartCoroutine(QuitGameAnimation());
    }

    IEnumerator QuitGameAnimation()
    {
        //TODO: funny stuff before quitting game?

        yield return new WaitForSeconds(0);

        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
