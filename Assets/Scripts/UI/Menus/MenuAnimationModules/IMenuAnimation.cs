using System;

// written by Claude Opus 5
// contract for anything that animates a menu opening or closing.
//
// same composition idea as IMenuFeedback: the menu doesn't know or care what's attached to it,
// it just tells everything to play and waits. drop a module on a menu to give it an animation,
// remove it to take it away, write a new class to invent a new kind.
//
// the one difference from IMenuFeedback is that animations take time, so the play methods hand
// back a callback instead of returning void and being done.
public interface IMenuAnimation
{
    /// <summary>
    /// Play the opening animation. Must invoke <paramref name="onComplete"/> exactly once when
    /// finished, or the menu will never become interactable.
    /// </summary>
    void PlayIn(Action onComplete);

    /// <summary>
    /// Play the closing animation. Same contract as <c>PlayIn</c>.
    /// </summary>
    void PlayOut(Action onComplete);

    /// <summary>
    /// Kill any running tweens and put everything back where the scene had it, so the menu can
    /// be replayed from a clean slate.
    /// </summary>
    void ResetState();
}
