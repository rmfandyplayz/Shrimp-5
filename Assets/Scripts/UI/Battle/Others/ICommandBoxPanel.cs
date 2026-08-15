// written by Claude Opus 5
// contract for one "page" of the command box (the dialogue text, the move buttons, the
// shrimp buttons, whatever gets added later).
//
// the point of this is that CommandBox doesn't need to know what panels exist. it just
// keeps a list of them, asks each one what mode it belongs to, and shows/hides accordingly.
// adding a 4th mode is: add an enum value, write a panel, drop it in the list.
public interface ICommandBoxPanel
{
    /// <summary>
    /// Which display mode this panel belongs to. Two panels must not claim the same one.
    /// </summary>
    CommandBoxMode Mode { get; }

    /// <summary>
    /// Brings the panel up.
    /// </summary>
    /// <param name="interactable">
    /// Whether the panel should actually take input, or just be visible. These come apart
    /// during move select, which keeps the dialogue panel on screen to show move details even
    /// though the player isn't "in" the dialogue at that point.
    /// </param>
    void Show(bool interactable);

    /// <summary>
    /// Takes the panel down. Expect this to deactivate the object, so don't start coroutines
    /// on a hidden panel.
    /// </summary>
    void Hide();
}
