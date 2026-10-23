using System.Collections.Generic;
using UnityEngine;

public static class PropSpawner
{
    public static void Spawn(Room room, RoomLayout layout, RoomStyle style, System.Random rng)
    {
        if (style.props == null) return;
        var t = layout.Tiles;
        var free = new List<Vector2Int>();
        for (int x = 1; x < t.GetLength(0) - 1; x++)
            for (int y = 1; y < t.GetLength(1) - 1; y++)
                if (t[x, y] == style.floor && !layout.Reserved[x, y]) free.Add(new Vector2Int(x, y));

        foreach (var rule in style.props)
        {
            int n = rng.Next(rule.countRange.x, rule.countRange.y + 1);
            for (int i = 0; i < n && free.Count > 0; i++)
            {
                int k = rng.Next(free.Count);
                var c = free[k]; free.RemoveAt(k);
                var go = Object.Instantiate(rule.prefab, room.transform);
                go.transform.localPosition = new Vector3(c.x - layout.Half.x, c.y - layout.Half.y, 0);
            }
        }
    }
}