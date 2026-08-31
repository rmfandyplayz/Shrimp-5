using DG.Tweening;
using TMPro;
using UnityEngine;

// written by andy
// animation moved out into MenuAnim_Transition modules by Claude Opus 5
// the credits screen. everything pops in from oversized while fading up, staggered down the list.
public class CreditsMenu : MenuBase
{
    [Header("legacy refs (only used by the migration button)")]
    [SerializeField] TextMeshProUGUI settingsText;
    [SerializeField] TextMeshProUGUI soundCreditsText;
    [SerializeField] CanvasGroup andyGroup;
    [SerializeField] CanvasGroup cassandraGroup;
    [SerializeField] CanvasGroup owenGroup;
    [SerializeField] CanvasGroup soundCreditsList;

    /// <summary>
    /// Rebuilds this menu's animation as MenuAnim_Transition modules, using the exact timings
    /// the hand written sequence had. Right click the component and pick this.
    /// </summary>
    [ContextMenu("Migrate animation to modules")]
    public void MigrateAnimationToModules()
    {
        MenuAnimMigration.ClearExisting(gameObject);

        // five entries all doing the same fade + shrink, 0.05s apart. leaving reverses the
        // order via the negative stagger, so the title goes last on the way out
        MenuAnimMigration.Add(gameObject, "credits entries",
            new Component[] { settingsText, andyGroup, cassandraGroup, owenGroup, soundCreditsText },
            MenuAnimMigration.Settings(0f, 0.05f,
                fade: MenuAnimMigration.Channel(0f, 0.8f, Ease.OutQuad),
                scale: MenuAnimMigration.Channel(3.5f, 0.8f, Ease.OutExpo)),
            MenuAnimMigration.Settings(0.25f, -0.05f,
                fade: MenuAnimMigration.Channel(0f, 0.4f, Ease.OutQuad),
                scale: MenuAnimMigration.Channel(2f, 0.4f, Ease.InQuart)));

        // the sound credits list is its own module purely because it scales from 1.15 rather
        // than 3.5 -- it's a grid of text, so blowing it up as far as the headings looked wrong
        MenuAnimMigration.Add(gameObject, "sound list",
            new Component[] { soundCreditsList },
            MenuAnimMigration.Settings(0.25f, 0f,
                fade: MenuAnimMigration.Channel(0f, 0.8f, Ease.OutQuad),
                scale: MenuAnimMigration.Channel(1.15f, 0.8f, Ease.OutExpo)),
            MenuAnimMigration.Settings(0f, 0f,
                fade: MenuAnimMigration.Channel(0f, 0.4f, Ease.OutQuad),
                scale: MenuAnimMigration.Channel(1.15f, 0.4f, Ease.InQuart)));

        MenuAnimMigration.MarkDirty(this);
        Debug.Log("[CreditsMenu] >> migrated to 2 transition modules.");
    }
}
