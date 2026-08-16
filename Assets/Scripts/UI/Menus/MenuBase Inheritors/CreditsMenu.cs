using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

// written by andy
// converted to data driven animation by Claude Opus 5
// the credits screen. everything pops in from oversized and fades, staggered down the list.
public class CreditsMenu : MenuBase
{
    [Header("animation targets (used to seed the step lists)")]
    [SerializeField] TextMeshProUGUI settingsText;
    [SerializeField] TextMeshProUGUI soundCreditsText;
    [SerializeField] CanvasGroup andyGroup;
    [SerializeField] CanvasGroup cassandraGroup;
    [SerializeField] CanvasGroup owenGroup;
    [SerializeField] CanvasGroup soundCreditsList;

    /// <summary>
    /// Refills the step lists with the animation this menu originally had hard coded.
    /// </summary>
    [ContextMenu("Restore original animation")]
    public void RestoreOriginalAnimation()
    {
        animateInSteps = new List<MenuAnimStep>();
        animateOutSteps = new List<MenuAnimStep>();

        // entering: each entry fades up from nothing while shrinking down from oversized,
        // 0.05s apart down the list
        AddPopIn("title", settingsText, 0f, 3.5f);
        AddPopIn("andy", andyGroup, 0.05f, 3.5f);
        AddPopIn("cassandra", cassandraGroup, 0.1f, 3.5f);
        AddPopIn("owen", owenGroup, 0.15f, 3.5f);
        AddPopIn("sound title", soundCreditsText, 0.2f, 3.5f);
        AddPopIn("sound list", soundCreditsList, 0.25f, 1.15f);

        // leaving: same thing in reverse order, faster, and it grows on the way out
        AddPopOut("sound list", soundCreditsList, 0f, 1.15f);
        AddPopOut("sound title", soundCreditsText, 0.05f, 2f);
        AddPopOut("owen", owenGroup, 0.1f, 2f);
        AddPopOut("cassandra", cassandraGroup, 0.15f, 2f);
        AddPopOut("andy", andyGroup, 0.2f, 2f);
        AddPopOut("title", settingsText, 0.25f, 2f);

        MenuAnimSeeding.MarkDirty(this);
    }

    private void AddPopIn(string label, Component target, float delay, float fromScale)
    {
        animateInSteps.Add(MenuAnimSeeding.Step($"{label} fade", target,
            MenuAnimProperty.Fade, 0f, delay, 0.8f, Ease.OutQuad));
        animateInSteps.Add(MenuAnimSeeding.Step($"{label} scale", target,
            MenuAnimProperty.Scale, fromScale, delay, 0.8f, Ease.OutExpo));
    }

    private void AddPopOut(string label, Component target, float delay, float toScale)
    {
        animateOutSteps.Add(MenuAnimSeeding.Step($"{label} fade", target,
            MenuAnimProperty.Fade, 0f, delay, 0.4f, Ease.OutQuad));
        animateOutSteps.Add(MenuAnimSeeding.Step($"{label} scale", target,
            MenuAnimProperty.Scale, toScale, delay, 0.4f, Ease.InQuart));
    }
}
