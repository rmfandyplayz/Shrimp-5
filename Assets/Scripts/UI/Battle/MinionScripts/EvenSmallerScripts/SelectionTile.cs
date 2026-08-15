using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// written by Claude Opus 5
// one button in the command box. used for BOTH move buttons and shrimp switch buttons --
// they're the same shape (icon + label + a thing to press), only the payload differs.
//
// it doesn't know what a move or a shrimp is. it gets handed a label, an icon and an id, and
// hands the id back when pressed. keeps all the battle knowledge in the panels above it.
public class SelectionTile : MonoBehaviour, ISelectHandler
{
    [Header("refs")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image icon;
    [SerializeField] private Button button;
    [SerializeField, Tooltip("optional. hidden when the tile has no icon to show")]
    private GameObject iconHolder;

    // what gets handed back when this tile is picked (a moveID or a shrimp instanceID)
    private string payloadId;

    private Action<string> onPicked;
    private Action<string> onHighlighted;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(HandlePressed);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandlePressed);
        }
    }

    /// <summary>
    /// Points this tile at something selectable.
    /// </summary>
    /// <param name="payloadId">handed back to <c>onPicked</c> when pressed (moveID / instanceID)</param>
    /// <param name="displayText">what the player reads on the button</param>
    /// <param name="iconSprite">the little type icon. pass null for no icon</param>
    /// <param name="onPicked">fired when the player confirms this tile</param>
    /// <param name="onHighlighted">fired when the tile becomes the selected one, so the
    /// text area can show details for whatever's under the cursor</param>
    public void Bind(string payloadId, string displayText, Sprite iconSprite,
        Action<string> onPicked, Action<string> onHighlighted = null)
    {
        this.payloadId = payloadId;
        this.onPicked = onPicked;
        this.onHighlighted = onHighlighted;

        if (label != null)
        {
            label.text = displayText;
        }

        SetIcon(iconSprite);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Swaps the type icon. Passing null hides the icon rather than leaving an empty square.
    /// </summary>
    public void SetIcon(Sprite iconSprite)
    {
        if (icon != null)
        {
            icon.sprite = iconSprite;
            icon.enabled = iconSprite != null;
        }

        if (iconHolder != null)
        {
            iconHolder.SetActive(iconSprite != null);
        }
    }

    /// <summary>
    /// Greys the tile out without hiding it. Used for things the player can see but can't
    /// pick, like a shrimp that's already fainted.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    /// <summary>
    /// The id this tile is currently standing for (a <c>moveID</c> or a shrimp
    /// <c>instanceID</c>), or null if it isn't bound to anything.
    /// </summary>
    public string GetPayloadId()
    {
        return payloadId;
    }

    /// <summary>
    /// Unbinds and hides the tile. Panels call this on their spare tiles when there are more
    /// buttons in the scene than there are things to show.
    /// </summary>
    public void Clear()
    {
        payloadId = null;
        onPicked = null;
        onHighlighted = null;
        gameObject.SetActive(false);
    }

    private void HandlePressed()
    {
        onPicked?.Invoke(payloadId);
    }

    // fired by the event system when the cursor lands on this tile. this is what drives
    // the move details showing up in the text area while you're browsing.
    // safe to sit alongside MenuInteractable -- unity dispatches to every ISelectHandler
    // on the object, not just the first one
    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        onHighlighted?.Invoke(payloadId);
    }
}
