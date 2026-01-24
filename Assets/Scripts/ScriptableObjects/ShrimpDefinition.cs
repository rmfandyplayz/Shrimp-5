using UnityEngine;

[CreateAssetMenu(menuName = "Battle/ShrimpDef")]
public class ShrimpDefinition : ScriptableObject
{
    [Header("Identity")]           
    public string displayName;       

    [Header("UI & Animation")]
    public string shrimpSpriteID;
    public string shrimpPortraitID;           
    public int maxHP;
    public int baseSpeed;
    public int baseAttack;
    public AbilityDefinition ability;
    public MoveDefinition[] moves = new MoveDefinition[3];
    
    }
