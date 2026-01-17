using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Shrimp")]
public class ShrimpDefinition : ScriptableObject
{
    [Header("Identity")]
    public string shrimapId;           // unique internal ID
    public string displayName;        // shown in UI

    [Header("UI & Animation")]
    public Sprite portrait;           // HUD portrait
    public string attackAnimationId;  // single attack animation
    public int maxHP;
    public int baseSpeed;
    public int baseAttack;
    public AbilityDefinition ability;
    public MoveDefinition[] moves = new MoveDefinition[3];
    
    }
