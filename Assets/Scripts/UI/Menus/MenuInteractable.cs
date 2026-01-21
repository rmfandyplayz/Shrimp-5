using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// written by andy
// a base class for composition scripts for interactable elements in menus
public class MenuInteractable : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    // actual ui element logic (load scene, open menu)
    // apparently with this u can drag n drop logic in inspector
    [SerializeField, Tooltip("what to do when the submit key is pressed while this object is selected?")]
    UnityEvent OnSubmitEvent;

    // cached menu item effects
    private IMenuFeedback[] feedbacks;

    private void Awake()
    {
        feedbacks = GetComponents<IMenuFeedback>();
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        foreach (IMenuFeedback f in feedbacks)
            f.OnDeselect();
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        foreach (IMenuFeedback f in feedbacks)
            f.OnSelect();
    }

    void ISubmitHandler.OnSubmit(BaseEventData eventData)
    {
        foreach (IMenuFeedback f in feedbacks)
            f.OnSubmit();

        OnSubmitEvent.Invoke();
    }
}
