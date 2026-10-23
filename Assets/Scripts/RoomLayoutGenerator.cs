using System.Collections.Generic;
using UnityEngine;

public class RoomLayout
{
    public TileDefinition[,] Tiles;
    public bool[,] Reserved;     // cases interdites aux hazards/props (devant les portes, centre)
    public Vector2Int Half;
}

public static class RoomLayoutGenerator
{
    static readonly Vector2Int[] Dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    public static RoomLayout Generate(RoomNode node, RoomStyle style, int level, System.Random rng)
    {
        int w = node.Size.x, h = node.Size.y;
        var half = new Vector2Int((w - 1) / 2, (h - 1) / 2);
        var layout = new RoomLayout { Tiles = new TileDefinition[w, h], Reserved = new bool[w, h], Half = half };
        var t = layout.Tiles;

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                t[x, y] = (x == 0 || y == 0 || x == w - 1 || y == h - 1) ? style.wall : style.floor;

        // Portes : case de porte = sol (la porte est un objet à part) + zone réservée devant
        var starts = new List<Vector2Int>();
        foreach (var d in node.Doors)
        {
            var door = new Vector2Int(half.x + d.x * half.x, half.y + d.y * half.y);
            t[door.x, door.y] = style.floor;
            var side = new Vector2Int(d.y, d.x);
            for (int i = 1; i <= 2; i++)
                for (int s = -1; s <= 1; s++)
                {
                    var c = door - d * i + side * s;
                    if (c.x > 0 && c.y > 0 && c.x < w - 1 && c.y < h - 1) layout.Reserved[c.x, c.y] = true;
                }
            starts.Add(door - d);
        }
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                layout.Reserved[half.x + x, half.y + y] = true;

        var origin = starts.Count > 0 ? starts[0] : half;

        // 1) Template dessiné à la main, sinon 2) hazards procéduraux
        var tpl = PickTemplate(style, node.Size, rng);
        if (tpl != null) tpl.ApplyTo(t, style.floor);
        else ApplyHazards(layout, style, node.Type, level, rng, origin);

        return layout;
    }

    static RoomTemplate PickTemplate(RoomStyle style, Vector2Int size, System.Random rng)
    {
        if (style.templates == null || rng.NextDouble() > style.templateChance) return null;
        var ok = new List<RoomTemplate>();
        foreach (var tp in style.templates) if (tp != null && tp.size == size) ok.Add(tp);
        return ok.Count > 0 ? ok[rng.Next(ok.Count)] : null;
    }

    static void ApplyHazards(RoomLayout layout, RoomStyle style, RoomType type, int level,
                             System.Random rng, Vector2Int origin)
    {
        if (style.hazards == null) return;
        var t = layout.Tiles;
        int w = t.GetLength(0), h = t.GetLength(1);

        foreach (var rule in style.hazards)
        {
            if (rule.tile == null || !rule.AppliesTo(type, level)) continue;

            int target = Mathf.RoundToInt((w - 2) * (h - 2) * rule.coverage);
            int placed = 0;
            for (int attempt = 0; attempt < 40 && placed < target; attempt++)
            {
                var cluster = GrowCluster(layout, style.floor, rng, rule.clusterSize);
                if (cluster.Count == 0) continue;

                foreach (var c in cluster) t[c.x, c.y] = rule.tile;

                if (rule.tile.Obstructs && !IsConnected(t, origin))
                {
                    foreach (var c in cluster) t[c.x, c.y] = style.floor;   // on annule
                    continue;
                }
                placed += cluster.Count;
            }
        }
    }

    static List<Vector2Int> GrowCluster(RoomLayout l, TileDefinition floor, System.Random rng, int size)
    {
        var t = l.Tiles;
        int w = t.GetLength(0), h = t.GetLength(1);
        bool Valid(Vector2Int c) => c.x > 0 && c.y > 0 && c.x < w - 1 && c.y < h - 1
                                    && t[c.x, c.y] == floor && !l.Reserved[c.x, c.y];

        var cells = new List<Vector2Int>();
        for (int i = 0; i < 20 && cells.Count == 0; i++)
        {
            var s = new Vector2Int(rng.Next(1, w - 1), rng.Next(1, h - 1));
            if (Valid(s)) cells.Add(s);
        }
        if (cells.Count == 0) return cells;

        for (int guard = 0; cells.Count < size && guard < size * 12; guard++)
        {
            var n = cells[rng.Next(cells.Count)] + Dirs[rng.Next(4)];
            if (Valid(n) && !cells.Contains(n)) cells.Add(n);
        }
        return cells;
    }

    /// <summary>Toutes les cases franchissables sont-elles atteignables depuis 'start' ?</summary>
    static bool IsConnected(TileDefinition[,] t, Vector2Int start)
    {
        int w = t.GetLength(0), h = t.GetLength(1);
        int open = 0;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (!t[x, y].Obstructs) open++;

        var seen = new bool[w, h];
        var q = new Queue<Vector2Int>();
        q.Enqueue(start); seen[start.x, start.y] = true;
        int reached = 0;
        while (q.Count > 0)
        {
            var c = q.Dequeue(); reached++;
            foreach (var d in Dirs)
            {
                var n = c + d;
                if (n.x < 0 || n.y < 0 || n.x >= w || n.y >= h) continue;
                if (seen[n.x, n.y] || t[n.x, n.y].Obstructs) continue;
                seen[n.x, n.y] = true; q.Enqueue(n);
            }
        }
        return reached == open;
    }
}

[CreateAssetMenu(menuName = "Dungeon/Room Template")]
public class RoomTemplate : ScriptableObject
{
    [System.Serializable] public struct Entry { public char symbol; public TileDefinition tile; }

    public Vector2Int size = new Vector2Int(15, 9);
    public Entry[] legend;
    [TextArea(8, 20)] public string ascii;   // ligne du haut = haut de la salle ; '.' = sol par défaut

    public void ApplyTo(TileDefinition[,] t, TileDefinition floor)
    {
        var lines = ascii.Replace("\r", "").Split('\n');
        int w = size.x, h = size.y;
        for (int row = 0; row < lines.Length && row < h; row++)
            for (int col = 0; col < lines[row].Length && col < w; col++)
            {
                int gx = col, gy = h - 1 - row;
                if (gx <= 0 || gy <= 0 || gx >= w - 1 || gy >= h - 1) continue; // périmètre = murs
                foreach (var e in legend)
                    if (e.symbol == lines[row][col] && e.tile != null) t[gx, gy] = e.tile;
            }
    }
}