using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// written by andy
// animation moved out into MenuAnim_Transition modules, keybind session wired, by Claude Opus 5
// the settings screen.
//
// besides its animation this is the screen that owns key rebinding. the important bit is that
// opening it drops the game to factory default controls and closing it commits whatever the
// player changed -- see KeybindEditSession for why.
public class SettingsMenu : MenuBase
{
    [Header("keybinding")]
    [SerializeField, Tooltip("leave blank to find one on this object or its children")]
    private KeybindEditSession keybindSession;

    [Header("legacy refs (only used by the migration button)")]
    [SerializeField] Image backgroundImage;
    [SerializeField] RectTransform blackPanel;
    [SerializeField] CanvasGroup scrollCanvasGroup;

    protected override void Awake()
    {
        base.Awake();

        if (keybindSession == null)
        {
            keybindSession = GetComponentInChildren<KeybindEditSession>(includeInactive: true);
        }

        if (keybindSession == null)
        {
            Debug.LogWarning($"[SettingsMenu] >> no KeybindEditSession found, so the key " +
                $"rebinding rows won't do anything. add one to this object.");
        }
    }

    /// <summary>
    /// Starts the keybind editing session. From here until the menu closes the game runs on
    /// factory default controls, so a player who has bound everything to one key can still
    /// navigate the screen that lets them fix it.
    /// </summary>
    public override void OnMenuOpened()
    {
        base.OnMenuOpened();

        keybindSession?.Begin();
    }

    /// <summary>
    /// Commits the player's rebinds, or factory resets them if they left with duplicates.
    /// </summary>
    public override void OnMenuClosed()
    {
        base.OnMenuClosed();

        keybindSession?.End();
    }

    /// <summary>
    /// Rebuilds this menu's animation as MenuAnim_Transition modules, using the exact timings
    /// the hand written sequence had. Right click the component and pick this.
    /// </summary>
    [ContextMenu("Migrate animation to modules")]
    public void MigrateAnimationToModules()
    {
        MenuAnimMigration.ClearExisting(gameObject);

        MenuAnimMigration.Add(gameObject, "black panel",
            new Component[] { blackPanel },
            MenuAnimMigration.Settings(0f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, -1440f, 0.5f, Ease.OutQuad)),
            MenuAnimMigration.Settings(0f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, -1440f, 0.5f, Ease.OutQuad)));

        MenuAnimMigration.Add(gameObject, "background art",
            new Component[] { backgroundImage },
            MenuAnimMigration.Settings(0.2f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.Y, -50f, 0.7f, Ease.OutQuint),
                fade: MenuAnimMigration.Channel(0f, 0.45f, Ease.OutQuad)),
            MenuAnimMigration.Settings(0f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.Y, -50f, 0.7f, Ease.OutQuint),
                fade: MenuAnimMigration.Channel(0f, 0.45f, Ease.OutQuad)));

        // world space X on purpose -- this one was written with DOMoveX, and an anchored
        // version would slide in from somewhere else entirely
        MenuAnimMigration.Add(gameObject, "scroll panel",
            new Component[] { scrollCanvasGroup },
            MenuAnimMigration.Settings(0.5f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, 850f, 0.5f, Ease.OutQuad, MenuMoveSpace.World),
                fade: MenuAnimMigration.Channel(0f, 0.5f, Ease.OutQuad)),
            MenuAnimMigration.Settings(0f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, 850f, 0.5f, Ease.OutQuad, MenuMoveSpace.World),
                fade: MenuAnimMigration.Channel(0f, 0.5f, Ease.OutQuad)));

        MenuAnimMigration.MarkDirty(this);
        Debug.Log("[SettingsMenu] >> migrated to 3 transition modules.");
    }
}
