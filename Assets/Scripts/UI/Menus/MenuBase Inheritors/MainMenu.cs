using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// written by andy
// animation moved out into MenuAnim_Transition modules by Claude Opus 5
// name might be confusing, but this is for the main part of the main menu
// where it shows play settings credits exit.
//
// there's no animation code left in here. the fields below are the old serialized references,
// kept only so the migration button can find what to point the modules at. once the migration
// has been run and checked, both they and this whole class can go.
public class MainMenu : MenuBase
{
    [Header("legacy refs (only used by the migration button)")]
    [SerializeField] List<TextMeshProUGUI> waterfallElements = new();
    [SerializeField] Image gameLogo;
    [SerializeField] RectTransform blackPanel;
    [SerializeField] Image backgroundArt;

    // sometimes i write code so bad i hope people will never trust my programming
    // skills again and therefore i can just design the ui without programming them

    // however i for some fking reason still like programming so i'm just screwing
    // myself up i suppose

    /// <summary>
    /// Rebuilds this menu's animation as MenuAnim_Transition modules, using the exact timings
    /// the hand written sequence had. Right click the component and pick this.
    ///
    /// Safe to re-run -- it clears the modules it made last time rather than stacking more.
    /// </summary>
    [ContextMenu("Migrate animation to modules")]
    public void MigrateAnimationToModules()
    {
        MenuAnimMigration.ClearExisting(gameObject);

        // the sliding black panel behind everything
        MenuAnimMigration.Add(gameObject, "black panel",
            new Component[] { blackPanel },
            MenuAnimMigration.Settings(0f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, 500f, 0.5f, Ease.OutQuad)),
            MenuAnimMigration.Settings(0.3f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, 500f, 0.5f, Ease.OutQuad)));

        MenuAnimMigration.Add(gameObject, "logo",
            new Component[] { gameLogo },
            MenuAnimMigration.Settings(0.4f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, 300f, 0.35f, Ease.OutCubic),
                fade: MenuAnimMigration.Channel(0f, 0.35f, Ease.OutQuad)),
            MenuAnimMigration.Settings(0.55f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, 300f, 0.2f, Ease.OutCubic),
                fade: MenuAnimMigration.Channel(0f, 0.2f, Ease.OutQuad)));

        // the four buttons cascading in 0.1s apart, hence "waterfall". leaving runs the
        // cascade backwards, which is what the negative stagger does
        MenuAnimMigration.Add(gameObject, "button waterfall",
            waterfallElements.ToArray(),
            MenuAnimMigration.Settings(0.5f, 0.1f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, 300f, 0.5f, Ease.OutQuad),
                fade: MenuAnimMigration.Channel(0f, 0.5f, Ease.OutQuad)),
            MenuAnimMigration.Settings(0.5f, -0.05f,
                move: MenuAnimMigration.Move(MenuMoveAxis.X, 300f, 0.2f, Ease.OutQuad),
                fade: MenuAnimMigration.Channel(0f, 0.2f, Ease.OutQuad)));

        MenuAnimMigration.Add(gameObject, "background art",
            new Component[] { backgroundArt },
            MenuAnimMigration.Settings(0.2f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.Y, -50f, 0.7f, Ease.OutQuint),
                fade: MenuAnimMigration.Channel(0f, 0.45f, Ease.OutQuad)),
            MenuAnimMigration.Settings(0.35f, 0f,
                move: MenuAnimMigration.Move(MenuMoveAxis.Y, -50f, 0.7f, Ease.OutQuint),
                fade: MenuAnimMigration.Channel(0f, 0.45f, Ease.OutQuad)));

        MenuAnimMigration.MarkDirty(this);
        Debug.Log("[MainMenu] >> migrated to 4 transition modules.");
    }
}
