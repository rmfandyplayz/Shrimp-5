using UnityEngine;

public class ShrimpDefinition : MonoBehaviour
{
    [SerializeField] private int maxHP;
    [SerializeField] private int baseSpeed;
    [SerializeField] private int baseAttack;

    public void SetHP(int value)
    {
        maxHP = value;
    }

    public void SetSpeed(int value)
    {
         baseSpeed = value;
    }
    
    public void SetAttack(int value)
    {
         baseAttack = value;
    }

    public int GetHP()
    {
        return maxHP;
    }

    public int GetSpeed()
    {
        return baseSpeed;
    }
    
    public int GetAttack()
    {
        return baseAttack;
    }
    }
