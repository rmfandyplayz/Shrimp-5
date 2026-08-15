using System;
using System.Collections.Generic;
using UnityEngine;

// handles all VISUAL aspects of the box at the bottom. whether that be choosing moves,
// going through dialogue, etc.
// written by andy
// restructured into a panel coordinator by Claude Opus 5
//
// this used to do everything itself. now it's a coordinator: it owns a list of panels
// (dialogue, move select, shrimp select) and just decides which one is up. each panel does
// its own thing. adding a 4th mode is: add a CommandBoxMode value, write a panel that
// implements ICommandBoxPanel, drop it in the panels list. no edits in here.
//
// handlers only ever talk to THIS script -- never to the panels directly.
public class CommandBox : MonoBehaviour
{
    [Header("panels")]
    [SerializeField, Tooltip("every page this command box can show. order doesn't matter")]
    private List<MonoBehaviour> panelScripts = new();

    [Header("specific panel refs")]
    [SerializeField, Tooltip("needed for dialogue + move details")]
    private DialoguePanel dialoguePanel;
    [SerializeField] private SelectionPanel moveSelectionPanel;
    [SerializeField] private SelectionPanel shrimpSelectionPanel;

    [Header("settings")]
    [SerializeField, Tooltip("keep the dialogue text visible during move select so move " +
        "details have somewhere to show")]
    private bool showDialogueDuringMoveSelect = true;

    // resolved at runtime from panelScripts
    private Dictionary<CommandBoxMode, ICommandBoxPanel> panels;

    private CommandBoxMode currentDisplayMode;

    // the moves currently on the buttons, so a highlight can be turned back into a definition
    private MoveDefinition[] boundMoves;

    private void Awake()
    {
        BuildPanelLookup();
    }

    private void Start()
    {
        // force the initial state. the old version early-returned when the requested mode
        // matched currentDisplayMode, which meant the very first switch to DIALOGUE_DISPLAY
        // (enum value 0, so also the default) did nothing and left every panel however the
        // scene happened to save it.
        //
        // this is in Start, not Awake, because the panels resolve their own CanvasGroups in
        // their Awake and unity doesn't promise which Awake runs first
        currentDisplayMode = CommandBoxMode.DIALOGUE_DISPLAY;
        ApplyDisplayMode(currentDisplayMode);
    }

    // turns the inspector's list of MonoBehaviours into a mode -> panel lookup.
    // same pattern BattleUIManager uses for its handlers, since unity can't serialize
    // interface references directly
    private void BuildPanelLookup()
    {
        panels = new Dictionary<CommandBoxMode, ICommandBoxPanel>();

        foreach (MonoBehaviour script in panelScripts)
        {
            if (script is ICommandBoxPanel panel)
            {
                if (panels.ContainsKey(panel.Mode))
                {
                    Debug.LogWarning($"[CommandBox] >> two panels both claim {panel.Mode}. " +
                        $"keeping the first one and ignoring {script.name}.");
                    continue;
                }

                panels.Add(panel.Mode, panel);
            }
            else
            {
                Debug.LogWarning($"[CommandBox] >> {script.name} does not implement ICommandBoxPanel. skipping!");
            }
        }
    }


    // TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER
    public void SwitchToShrimpSelect()
    {
        SwitchCommandBoxDisplayMode(CommandBoxMode.SHRIMP_SELECT);
    }

    public void SwitchToMoveSelect()
    {
        SwitchCommandBoxDisplayMode(CommandBoxMode.MOVE_SELECT);
    }
    public void SwitchToDialogue()
    {
        SwitchCommandBoxDisplayMode(CommandBoxMode.DIALOGUE_DISPLAY);
    }
    // TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER TESTING DELETE LATER


    /// <summary>
    /// Flips the command box to a different page. No-ops if it's already on that one.
    /// </summary>
    public void SwitchCommandBoxDisplayMode(CommandBoxMode newDisplayMode)
    {
        if (newDisplayMode == currentDisplayMode)
            return;

        currentDisplayMode = newDisplayMode;
        ApplyDisplayMode(newDisplayMode);
    }

    /// <summary>
    /// Which page is currently up. Input code uses this to work out what Back should do.
    /// </summary>
    public CommandBoxMode GetDisplayMode()
    {
        return currentDisplayMode;
    }

    // shows the panel for the requested mode and hides the rest.
    // the one exception is the dialogue panel during move select -- it stays up (but not
    // interactable) so it can show the details of whichever move is highlighted
    private void ApplyDisplayMode(CommandBoxMode mode)
    {
        foreach (KeyValuePair<CommandBoxMode, ICommandBoxPanel> entry in panels)
        {
            bool isTarget = entry.Key == mode;
            bool isDialogueTagalong = showDialogueDuringMoveSelect
                && mode == CommandBoxMode.MOVE_SELECT
                && entry.Key == CommandBoxMode.DIALOGUE_DISPLAY;

            if (isTarget)
            {
                entry.Value.Show(interactable: true);
            }
            else if (isDialogueTagalong)
            {
                entry.Value.Show(interactable: false);
            }
            else
            {
                entry.Value.Hide();
            }
        }
    }


    // dialogue ============================================================================

    /// <summary>
    /// Types a line out in the text area.
    /// <paramref name="onComplete"/> fires when the last word lands (or when the player
    /// skips), which is how handlers know the line has been read.
    /// </summary>
    public void SetDialogue(string textToDisplay, Action onComplete = null)
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning("[CommandBox] >> no dialoguePanel assigned, can't say anything.");
            onComplete?.Invoke();
            return;
        }

        // saying something means the dialogue is what's on screen. this also guarantees the
        // panel's object is active, which it has to be before it can run the typing coroutine
        SwitchCommandBoxDisplayMode(CommandBoxMode.DIALOGUE_DISPLAY);

        dialoguePanel.SetDialogue(textToDisplay, onComplete);
    }

    /// <summary>
    /// Puts a line up instantly, no typewriter. For prompts the player shouldn't wait on.
    /// </summary>
    public void SetDialogueInstant(string textToDisplay)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetDialogueInstant(textToDisplay);
        }
    }

    /// <summary>
    /// Stops the typing coroutine and displays the full text, firing whatever callback was
    /// waiting on the line. Safe to call when nothing is typing.
    /// </summary>
    public void SkipDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SkipDialogue();
        }
    }

    /// <summary>
    /// Whether a line is still typing itself out.
    /// </summary>
    public bool IsTypingDialogue()
    {
        return dialoguePanel != null && dialoguePanel.IsTyping();
    }


    // selection ===========================================================================

    /// <summary>
    /// Puts a shrimp's moves on the move buttons.
    /// <paramref name="onPicked"/> gets the chosen <c>moveID</c>.
    /// </summary>
    public void PopulateMoveTiles(MoveDefinition[] moves, Action<string> onPicked)
    {
        if (moveSelectionPanel == null)
        {
            Debug.LogWarning("[CommandBox] >> no moveSelectionPanel assigned.");
            return;
        }

        boundMoves = moves;

        List<SelectionEntry> entries = new();

        if (moves != null)
        {
            foreach (MoveDefinition move in moves)
            {
                if (move == null)
                    continue;

                // TODO: revisit alongside MoveCategory once owen settles how categories are stored
                entries.Add(new SelectionEntry(
                    move.moveID,
                    !string.IsNullOrEmpty(move.displayName) ? move.displayName : move.name,
                    MoveCategories.GetIcon(move)));
            }
        }

        moveSelectionPanel.Bind(entries, onPicked, ShowMoveDetailsFor);
    }

    /// <summary>
    /// Puts the player's benched shrimp on the switch buttons.
    /// <paramref name="onPicked"/> gets the chosen shrimp's <c>instanceID</c>.
    /// </summary>
    public void PopulateShrimpTiles(IList<UIShrimpState> team, Action<string> onPicked)
    {
        if (shrimpSelectionPanel == null)
        {
            Debug.LogWarning("[CommandBox] >> no shrimpSelectionPanel assigned.");
            return;
        }

        List<SelectionEntry> entries = new();

        if (team != null)
        {
            foreach (UIShrimpState shrimp in team)
            {
                if (shrimp == null)
                    continue;

                // a fainted shrimp still shows up, just greyed out
                bool alive = shrimp.currentHP > 0;

                entries.Add(new SelectionEntry(
                    shrimp.shrimpUniqueId,
                    shrimp.displayName,
                    UISprites.Get(shrimp.pfpId),
                    alive));
            }
        }

        shrimpSelectionPanel.Bind(entries, onPicked);
    }

    /// <summary>
    /// Shows a move's full writeup in the text area (damage/healing, effect, icon, blurb).
    /// </summary>
    public void ShowMoveDetails(MoveDefinition move)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.ShowMoveDetails(move);
        }
    }

    // fired when the cursor lands on a move tile. tiles only know their moveID, so turn that
    // back into the definition we bound and push its details into the text area
    private void ShowMoveDetailsFor(string moveId)
    {
        if (boundMoves == null)
            return;

        foreach (MoveDefinition move in boundMoves)
        {
            if (move != null && move.moveID == moveId)
            {
                ShowMoveDetails(move);
                return;
            }
        }
    }
}

public enum CommandBoxMode
{
    DIALOGUE_DISPLAY,
    MOVE_SELECT,
    SHRIMP_SELECT
}
