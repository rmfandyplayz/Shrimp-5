using System.Collections.Generic;
using UnityEngine;

public class ShrimpSnapshot : MonoBehaviour
{
    public string displayName;
    public Sprite portrait;
    public int currentHp;
    public int maxHp;
    public int attack;
    public int speed;

    public List<StatusSnapshot> statuses;
    public List<MoveSnapshot> availableMoves;
    public AbilitySnapshot ability;

    public static ShrimpSnapshot CreateShrimpSnapshot(ShrimpState shrimp)
{
    var snapshot = new ShrimpSnapshot();

    snapshot.displayName = shrimp.definition.displayName;
    snapshot.portrait = shrimp.definition.portrait;
    snapshot.currentHp = shrimp.currentHP;
    snapshot.maxHp = shrimp.GetHP();
    snapshot.attack = shrimp.GetAttack();
    snapshot.speed = shrimp.GetSpeed();

    // flatten statuses
    snapshot.statuses = new List<StatusSnapshot>();
    foreach(var status in shrimp.statuses)
        snapshot.statuses.Add(StatusSnapshot.CreateStatusSnapshot(status));

    // flatten moves
    snapshot.availableMoves = new List<MoveSnapshot>();
    foreach(var move in shrimp.definition.moves)
        snapshot.availableMoves.Add(MoveSnapshot.CreateMoveSnapshot(move));

    // flatten abilities
    snapshot.ability = AbilitySnapshot.CreateAbilitytSnapshot(shrimp.definition.ability);

    return snapshot;
}
}
