using System;
using System.Collections.Generic;
using Shrimp5.UIContract;
using UnityEngine;

public class BattleUIModel : MonoBehaviour, IBattleUIModel
{
    public event Action Changed;
    private BattleSnapshot snapshot;

    void Start()
    {
        snapshot = new BattleSnapshot();
        snapshot.battleMode = BattleUIMode.ChoosingAction;
    }

    // unused for now
    public List<BattleUIEvent> DrainUIEvents()
    {
        return new List<BattleUIEvent>();
    }

    public BattleSnapshot GetSnapshot()
    {
        return snapshot;
    }

    public void SetSnapshot(BattleSnapshot newSnapshot)
    {
        this.snapshot = newSnapshot;
        Changed?.Invoke();
    }

}
