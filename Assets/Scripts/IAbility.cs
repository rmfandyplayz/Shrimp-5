using UnityEngine;

public interface IAbility
{
    public void WhenSwitchIn();
    public void WhenHit();
    public void WhenDie();
    public void EveryTurn();
}
