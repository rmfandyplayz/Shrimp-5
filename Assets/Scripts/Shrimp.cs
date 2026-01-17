using UnityEngine;

public class Shrimp : MonoBehaviour
{
    [SerializeField] private int hpStat;
    [SerializeField] private int speedStat;
    [SerializeField] private int attackStat;
    private IAbility ability;
    private IMoves moves;

    void Start()
    {
        ability = GetComponent<IAbility>();
        moves = GetComponent<IMoves>();
    }

    public void ChangeHP(int hpChange)
    {
        hpStat += hpChange;
    }
    
    public void ChangeSpeed(int speedChange)
    {
        speedStat += speedChange;
    }

    public void ChangeAttack(int attackChange)
    {
        attackStat += attackChange;
    }

    public int GetHP()
    {
        return hpStat;
    }

    public int GetSpeed()
    {
        return speedStat;
    }
    
    public int GetAttack()
    {
        return attackStat;
    }
}
