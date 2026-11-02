using System;
using UnityEngine;

/// <summary>Argent du joueur. À ajouter sur le même GameObject que PlayerHealth.</summary>
public class PlayerWallet : MonoBehaviour
{
    [SerializeField] int startingCoins = 0;

    public int Coins { get; private set; }
    public event Action<int> Changed;

    void Awake() => Coins = startingCoins;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        Changed?.Invoke(Coins);
    }

    public bool TrySpend(int amount)
    {
        if (amount > Coins) return false;
        Coins -= amount;
        Changed?.Invoke(Coins);
        return true;
    }
}