using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// written by Claude Opus 5
// a group of SelectionTiles the player picks from. one of these for the move buttons and
// another for the shrimp switch buttons -- same script, just different data pushed into it.
//
// it grows/shrinks the tile list to fit whatever it's given, so a shrimp with 2 usable moves
// or a team of 5 both work without touching the scene.
public class SelectionPanel : MonoBehaviour, ICommandBoxPanel
{
    [Header("what mode does this panel belong to?")]
    [SerializeField] private CommandBoxMode mode;

    [Header("refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField, Tooltip("where new tiles get spawned under. defaults to this transform")]
    private Transform tileParent;
    [SerializeField, Tooltip("duplicated when there are more entries than tiles already in the scene")]
    private SelectionTile tilePrefab;

    [Header("tiles already placed in the scene")]
    [SerializeField] private List<SelectionTile> tiles = new();

    CommandBoxMode ICommandBoxPanel.Mode => mode;

    // how many tiles are actually bound to something right now
    private int activeTileCount;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (tileParent == null)
            tileParent = transform;
    }

    /// <summary>
    /// Fills the panel with one tile per entry, spawning or hiding tiles as needed.
    /// </summary>
    /// <param name="entries">what to show. see <c>SelectionEntry</c></param>
    /// <param name="onPicked">gets the entry's id when the player confirms it</param>
    /// <param name="onHighlighted">gets the entry's id when the cursor lands on it</param>
    public void Bind(IList<SelectionEntry> entries, Action<string> onPicked,
        Action<string> onHighlighted = null)
    {
        int wanted = entries != null ? entries.Count : 0;

        EnsureTileCount(wanted);

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == null)
                continue;

            if (i < wanted)
            {
                SelectionEntry entry = entries[i];
                tiles[i].Bind(entry.id, entry.label, entry.icon, onPicked, onHighlighted);
                tiles[i].SetInteractable(entry.interactable);
            }
            else
            {
                tiles[i].Clear();
            }
        }

        activeTileCount = wanted;
    }

    void ICommandBoxPanel.Show(bool interactable)
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
            canvasGroup.alpha = 1f;
        }

        if (interactable)
        {
            SelectFirstTile();
        }
    }

    void ICommandBoxPanel.Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Puts the cursor on the first usable tile. Called on show, but also worth calling
    /// after a rebind if the panel was already open.
    /// </summary>
    public void SelectFirstTile()
    {
        if (EventSystem.current == null)
            return;

        GameObject first = GetFirstSelectable();

        if (first == null)
        {
            Debug.LogWarning($"[SelectionPanel] >> {name} has nothing to select. was Bind() called?");
            return;
        }

        // unity wants the deselect first or it sometimes ignores the new selection
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(first);
    }

    // first tile that's actually bound AND on screen. the activeInHierarchy check matters
    // because Bind can run while the panel is still hidden
    private GameObject GetFirstSelectable()
    {
        for (int i = 0; i < activeTileCount && i < tiles.Count; i++)
        {
            if (tiles[i] != null && tiles[i].gameObject.activeInHierarchy)
                return tiles[i].gameObject;
        }

        return null;
    }

    // spawns more tiles off the prefab if we've been handed more entries than we have buttons.
    // never shrinks the pool -- spare tiles get hidden by Bind and reused next time
    private void EnsureTileCount(int wanted)
    {
        if (wanted <= tiles.Count)
            return;

        if (tilePrefab == null)
        {
            Debug.LogWarning($"[SelectionPanel] >> {name} needs {wanted} tiles but only has " +
                $"{tiles.Count} and no tilePrefab to make more. the extras won't show up.");
            return;
        }

        while (tiles.Count < wanted)
        {
            SelectionTile spawned = Instantiate(tilePrefab, tileParent);
            spawned.name = $"{tilePrefab.name} ({tiles.Count + 1})";
            tiles.Add(spawned);
        }
    }
}

/// <summary>
/// One row's worth of data for a <c>SelectionPanel</c>.
///
/// Deliberately dumb: whoever builds these (<c>CommandBox</c>) is the one that knows about
/// moves and shrimp. The panel and its tiles just render what they're given, which is why the
/// same panel script works for both menus.
/// </summary>
public struct SelectionEntry
{
    public string id;      // moveID, or a shrimp instanceID
    public string label;
    public Sprite icon;
    public bool interactable;   // false = visible but greyed out (fainted shrimp, etc)

    public SelectionEntry(string id, string label, Sprite icon, bool interactable = true)
    {
        this.id = id;
        this.label = label;
        this.icon = icon;
        this.interactable = interactable;
    }
}
