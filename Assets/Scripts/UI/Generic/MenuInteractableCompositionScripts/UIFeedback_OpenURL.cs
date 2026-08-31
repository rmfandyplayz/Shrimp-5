using UnityEngine;

// very self explanatory
// written by andy
public class UIFeedback_OpenURL : MonoBehaviour, IMenuFeedback
{
    [SerializeField] private string url;
    void IMenuFeedback.OnDeselect() { }

    void IMenuFeedback.OnSelect() { }

    void IMenuFeedback.OnSubmit()
    {
        Application.OpenURL(url);
    }
}
