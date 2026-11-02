using TMPro;
using UnityEngine;

public class CoinCounterUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] string format = "{0}";

    PlayerWallet wallet;

    void Start()
    {
        wallet = FindFirstObjectByType<PlayerWallet>();
        if (wallet == null) { Debug.LogWarning("CoinCounterUI : aucun PlayerWallet trouvé"); return; }
        wallet.Changed += Refresh;
        Refresh(wallet.Coins);
    }

    void OnDestroy()
    {
        if (wallet != null) wallet.Changed -= Refresh;
    }

    void Refresh(int coins) => label.text = string.Format(format, coins);
}