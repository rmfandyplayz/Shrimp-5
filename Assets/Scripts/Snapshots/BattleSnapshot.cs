using UnityEngine;

public class BattleSnapshot : MonoBehaviour
{
    public ShrimpSnapshot playerShrimp;
    public ShrimpSnapshot enemyShrimp;    

    public bool isPlayerTurn;             
    public string turnText;      

    public BattleSnapshot CreateBattleSnapshot(ShrimpState player, ShrimpState enemy, bool playerTurn)
    {
        var snapshot = new BattleSnapshot();
        playerShrimp = ShrimpSnapshot.CreateShrimpSnapshot(player);
        enemyShrimp = ShrimpSnapshot.CreateShrimpSnapshot(enemy);
        isPlayerTurn = playerTurn;
        snapshot.turnText = isPlayerTurn ? "Your move" : "Enemy move";

        return snapshot;
    }         
}
