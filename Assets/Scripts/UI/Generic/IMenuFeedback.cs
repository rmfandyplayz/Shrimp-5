// written by andy
// this acts as a contract to signal to buttons/selectables on their standard behavior
// given certain events (selecting them, deselecting them, pressing them)
public interface IMenuFeedback
{
    void OnSelect();
    void OnDeselect();
    void OnSubmit();
}
