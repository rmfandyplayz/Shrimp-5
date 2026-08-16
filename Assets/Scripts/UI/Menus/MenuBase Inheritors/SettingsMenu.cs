using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// written by andy
// converted to data driven animation, and wired to the keybind session, by Claude Opus 5
// animations for settings menu
//
// besides the animation this is the screen that owns key rebinding. the important bit is that
// opening it drops the game to factory default controls and closing it commits whatever the
// player changed -- see KeybindEditSession for why.
public class SettingsMenu : MenuBase
{
    [Header("keybinding")]
    [SerializeField, Tooltip("leave blank to find one on this object or its children")]
    private KeybindEditSession keybindSession;

    [Header("animation targets (used to seed the step lists)")]
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
    /// Refills the step lists with the animation this menu originally had hard coded.
    /// </summary>
    [ContextMenu("Restore original animation")]
    public void RestoreOriginalAnimation()
    {
        animateInSteps = new List<MenuAnimStep>
        {
            MenuAnimSeeding.Step("black panel", blackPanel, MenuAnimProperty.AnchorPosX, -1440f, 0f, 0.5f, Ease.OutQuad),

            MenuAnimSeeding.Step("art slide", backgroundImage, MenuAnimProperty.AnchorPosY, -50f, 0.2f, 0.7f, Ease.OutQuint),
            MenuAnimSeeding.Step("art fade", backgroundImage, MenuAnimProperty.Fade, 0f, 0.2f, 0.45f, Ease.OutQuad),

            // world space on purpose -- this one was written with DOMoveX and an anchored
            // version would land somewhere else
            MenuAnimSeeding.Step("scroll slide", scrollCanvasGroup, MenuAnimProperty.WorldMoveX, 850f, 0.5f, 0.5f, Ease.OutQuad),
            MenuAnimSeeding.Step("scroll fade", scrollCanvasGroup, MenuAnimProperty.Fade, 0f, 0.5f, 0.5f, Ease.OutQuad)
        };

        // everything leaves at once here, no stagger
        animateOutSteps = new List<MenuAnimStep>
        {
            MenuAnimSeeding.Step("black panel", blackPanel, MenuAnimProperty.AnchorPosX, -1440f, 0f, 0.5f, Ease.OutQuad),

            MenuAnimSeeding.Step("art slide", backgroundImage, MenuAnimProperty.AnchorPosY, -50f, 0f, 0.7f, Ease.OutQuint),
            MenuAnimSeeding.Step("art fade", backgroundImage, MenuAnimProperty.Fade, 0f, 0f, 0.45f, Ease.OutQuad),

            MenuAnimSeeding.Step("scroll slide", scrollCanvasGroup, MenuAnimProperty.WorldMoveX, 850f, 0f, 0.5f, Ease.OutQuad),
            MenuAnimSeeding.Step("scroll fade", scrollCanvasGroup, MenuAnimProperty.Fade, 0f, 0f, 0.5f, Ease.OutQuad)
        };

        MenuAnimSeeding.MarkDirty(this);
    }
}
