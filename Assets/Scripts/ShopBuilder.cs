using System.Collections.Generic;
using UnityEngine;

/// <summary>Construit le contenu d'une salle Shop : vendeur + cartes.</summary>
public static class ShopBuilder
{
    public static void Build(Room room, BonusCard[] pool, int count, System.Random rng)
    {
        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning("ShopBuilder : aucune carte dans 'shopCards' sur FloorBuilder.");
            return;
        }

        var size = room.Node.Size;
        var half = new Vector2Int((size.x - 1) / 2, (size.y - 1) / 2);

        // --- Vendeur (carré violet, à remplacer par un vrai sprite) ---
        var keeper = new GameObject("ShopKeeper");
        keeper.transform.SetParent(room.transform, false);
        keeper.transform.localPosition = new Vector3(0, Mathf.Min(2, half.y - 1), 0);
        keeper.transform.localScale = new Vector3(0.9f, 1.2f, 1f);
        var sr = keeper.AddComponent<SpriteRenderer>();
        sr.sprite = EnemyFactory.Square;
        sr.color = new Color(0.6f, 0.3f, 0.8f);
        sr.sortingOrder = 2;

        // --- Cartes : tirage sans doublon (rng de la salle = reproductible avec la seed) ---
        var cards = new List<BonusCard>();
        foreach (var c in pool) if (c != null) cards.Add(c);

        count = Mathf.Min(count, cards.Count);
        float spacing = Mathf.Min(3f, (half.x * 2f - 2f) / Mathf.Max(1, count));

        for (int i = 0; i < count; i++)
        {
            int k = rng.Next(cards.Count);
            var card = cards[k];
            cards.RemoveAt(k);

            float x = (i - (count - 1) * 0.5f) * spacing;
            ShopItem.Create(room.transform, new Vector3(x, 0f, 0f), card);
        }
    }
}