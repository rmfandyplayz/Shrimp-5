using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// written by andy
// converted to data driven animation by Claude Opus 5
// name might be confusing, but this is for the main part of the main menu
// where it shows play settings credits exit.
//
// the animation itself now lives in MenuBase's step lists. the fields below are only kept so
// the scene's existing references survive and so "Restore original animation" has something
// to point the steps at.
public class MainMenu : MenuBase
{
    [Header("animation targets (used to seed the step lists)")]
    [SerializeField] List<TextMeshProUGUI> waterfallElements = new();
    [SerializeField] Image gameLogo;
    [SerializeField] RectTransform blackPanel;
    [SerializeField] Image backgroundArt;

    // sometimes i write code so bad i hope people will never trust my programming
    // skills again and therefore i can just design the ui without programming them

    // however i for some fking reason still like programming so i'm just screwing
    // myself up i suppose

    /// <summary>
    /// Refills the step lists with the waterfall animation this menu originally had hard coded.
    ///
    /// Right click the component and pick this to get back to the known good version -- either
    /// to set it up the first time, or after experimenting in the inspector.
    /// </summary>
    [ContextMenu("Restore original animation")]
    public void RestoreOriginalAnimation()
    {
        animateInSteps = BuildInSteps();
        animateOutSteps = BuildOutSteps();

        MenuAnimSeeding.MarkDirty(this);
    }

    private List<MenuAnimStep> BuildInSteps()
    {
        List<MenuAnimStep> steps = new()
        {
            MenuAnimSeeding.Step("black panel", blackPanel, MenuAnimProperty.AnchorPosX, 500f, 0f, 0.5f, Ease.OutQuad),

            MenuAnimSeeding.Step("logo slide", gameLogo, MenuAnimProperty.AnchorPosX, 300f, 0.4f, 0.35f, Ease.OutCubic),
            MenuAnimSeeding.Step("logo fade", gameLogo, MenuAnimProperty.Fade, 0f, 0.4f, 0.35f, Ease.OutQuad),

            MenuAnimSeeding.Step("art slide", backgroundArt, MenuAnimProperty.AnchorPosY, -50f, 0.2f, 0.7f, Ease.OutQuint),
            MenuAnimSeeding.Step("art fade", backgroundArt, MenuAnimProperty.Fade, 0f, 0.2f, 0.45f, Ease.OutQuad)
        };

        // the buttons cascade in 0.1s apart, hence "waterfall"
        for (int i = 0; i < waterfallElements.Count; i++)
        {
            float delay = 0.5f + (i * 0.1f);

            steps.Add(MenuAnimSeeding.Step($"button {i + 1} slide", waterfallElements[i],
                MenuAnimProperty.AnchorPosX, 300f, delay, 0.5f, Ease.OutQuad));
            steps.Add(MenuAnimSeeding.Step($"button {i + 1} fade", waterfallElements[i],
                MenuAnimProperty.Fade, 0f, delay, 0.5f, Ease.OutQuad));
        }

        return steps;
    }

    private List<MenuAnimStep> BuildOutSteps()
    {
        List<MenuAnimStep> steps = new()
        {
            MenuAnimSeeding.Step("black panel", blackPanel, MenuAnimProperty.AnchorPosX, 500f, 0.3f, 0.5f, Ease.OutQuad),

            MenuAnimSeeding.Step("logo slide", gameLogo, MenuAnimProperty.AnchorPosX, 300f, 0.55f, 0.2f, Ease.OutCubic),
            MenuAnimSeeding.Step("logo fade", gameLogo, MenuAnimProperty.Fade, 0f, 0.55f, 0.2f, Ease.OutQuad),

            MenuAnimSeeding.Step("art slide", backgroundArt, MenuAnimProperty.AnchorPosY, -50f, 0.35f, 0.7f, Ease.OutQuint),
            MenuAnimSeeding.Step("art fade", backgroundArt, MenuAnimProperty.Fade, 0f, 0.35f, 0.45f, Ease.OutQuad)
        };

        // leaving runs the waterfall backwards -- last button first
        for (int i = 0; i < waterfallElements.Count; i++)
        {
            float delay = 0.5f - (i * 0.05f);

            steps.Add(MenuAnimSeeding.Step($"button {i + 1} slide", waterfallElements[i],
                MenuAnimProperty.AnchorPosX, 300f, delay, 0.2f, Ease.OutQuad));
            steps.Add(MenuAnimSeeding.Step($"button {i + 1} fade", waterfallElements[i],
                MenuAnimProperty.Fade, 0f, delay, 0.2f, Ease.OutQuad));
        }

        return steps;
    }
}
