using System;
using System.Collections.Generic;
using Shrimp5.UIContract;
using Unity.VisualScripting;
using UnityEngine;

public class BattleUIModel : MonoBehaviour, IBattleUIModel
{
    public event Action Changed;
    private BattleSnapshot snapshot;

    void Awake()
    {
        snapshot = new BattleSnapshot();
        snapshot.buttons = new List<ButtonData>();
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
