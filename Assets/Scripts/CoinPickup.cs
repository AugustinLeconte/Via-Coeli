using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    int amount = 1;
    float pickupTime;

    static CoinPickup prefab;

    public static CoinPickup Spawn(Vector2 pos, int amount = 1)
    {
        // Chargé une seule fois depuis Assets/Resources/CoinPickup.prefab
        if (prefab == null) prefab = Resources.Load<CoinPickup>("CoinPickup");
        if (prefab == null)
        {
            Debug.LogWarning("CoinPickup : prefab introuvable (Assets/Resources/CoinPickup.prefab).");
            return null;
        }

        pos += Random.insideUnitCircle * 0.4f;

        var room = Room.FindAt(pos);
        var c = Instantiate(prefab, pos, Quaternion.identity, room != null ? room.transform : null);
        c.amount = amount;
        c.pickupTime = Time.time + 0.3f;
        return c;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < pickupTime) return;
        var wallet = other.GetComponentInParent<PlayerWallet>();
        if (wallet == null) return;
        wallet.Add(amount);
        Destroy(gameObject);
    }
}