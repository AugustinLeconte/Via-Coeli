using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Une carte en vente. Créée par ShopBuilder, aucun prefab nécessaire.</summary>
public class ShopItem : MonoBehaviour
{
    BonusCard card;
    TextMeshPro priceLabel, infoLabel;
    PlayerWallet playerInRange;
    InputAction interact;

    void Awake()
    {
        interact = new InputAction("Interact", InputActionType.Button);
        interact.AddBinding("<Keyboard>/e");
        interact.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable()  => interact.Enable();
    void OnDisable() => interact.Disable();
    void OnDestroy() => interact.Dispose();

    public static ShopItem Create(Transform parent, Vector3 localPos, BonusCard card)
    {
        var root = new GameObject("ShopItem_" + card.displayName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;

        // Zone d'interaction (trigger)
        var col = root.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.6f, 1.6f);

        // Visuel
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        if (card.icon != null) { sr.sprite = card.icon; }
        else
        {
            sr.sprite = EnemyFactory.Square;
            sr.color = card.color;
            visual.transform.localScale = new Vector3(0.8f, 1.1f, 1f);   // format "carte"
        }

        var item = root.AddComponent<ShopItem>();
        item.card = card;
        item.priceLabel = MakeLabel(root.transform, new Vector3(0, -0.95f, 0), 4f, new Vector2(4, 1));
        item.infoLabel  = MakeLabel(root.transform, new Vector3(0, 1.4f, 0), 2.5f, new Vector2(6, 3));
        item.priceLabel.text = card.price.ToString();
        item.infoLabel.text = "";
        return item;
    }

    static TextMeshPro MakeLabel(Transform parent, Vector3 localPos, float size, Vector2 rect)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var t = go.AddComponent<TextMeshPro>();
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.rectTransform.sizeDelta = rect;
        t.GetComponent<MeshRenderer>().sortingOrder = 10;
        return t;
    }

    void Update()
    {
        if (playerInRange == null) return;

        bool canAfford = playerInRange.Coins >= card.price;
        priceLabel.color = canAfford ? Color.white : new Color(1f, 0.3f, 0.3f);

        if (interact.WasPressedThisFrame() && playerInRange.TrySpend(card.price))
        {
            card.Apply(playerInRange.gameObject);
            Destroy(gameObject);   // vendu
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var w = other.GetComponentInParent<PlayerWallet>();
        if (w == null) return;
        playerInRange = w;
        infoLabel.text = $"<b>{card.displayName}</b>\n{card.description}\n[E] Acheter";
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerWallet>() != playerInRange) return;
        playerInRange = null;
        infoLabel.text = "";
        priceLabel.color = Color.white;
    }
}