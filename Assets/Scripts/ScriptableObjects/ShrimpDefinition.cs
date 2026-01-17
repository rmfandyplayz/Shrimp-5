using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Shrimp")]
public class ShrimpDefinition : ScriptableObject
{
    [Header("Identity")]
    public string shrimapId;           
    public string displayName;       

    [Header("UI & Animation")]
    public Sprite portrait;           
    public string attackAnimationId;  
    public int maxHP;
    public int baseSpeed;
    public int baseAttack;
    public AbilityDefinition ability;
    public MoveDefinition[] moves = new MoveDefinition[3];
    
    }
