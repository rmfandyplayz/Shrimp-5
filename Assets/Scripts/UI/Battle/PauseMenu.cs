using System;
using UnityEngine;

// written by andy
// handles the battle pause menu
public class PauseMenu : MenuBase
{
    [SerializeField] BattleController battleController;

    private bool isEnabled = false;

    public override void OnBackPressed()
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        battleController.PauseToggle();

        AnimateOut(() =>
        {
            // disable gameobject if animate out doesn't handle it well enough
        });
    }

    public void OpenSettings()
    {
        //TODO: implement later
    }

    public void QuitToMainMenu()
    {
        //TODO: implement later
    }

    public override void AnimateIn(Action onComplete)
    {
        base.AnimateIn(onComplete);
    }

    public override void AnimateOut(Action onComplete)
    {
        base.AnimateOut(onComplete);
    }

    public bool GetEnabled()
    {
        return isEnabled;
    }
}
