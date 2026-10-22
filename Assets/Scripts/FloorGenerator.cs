using System.Collections.Generic;
using UnityEngine;

public enum RoomType { Start, Normal, Treasure, Shop, Boss }

public class RoomNode
{
    public Vector2Int GridPos;
    public RoomType Type = RoomType.Normal;
    public int DistanceFromStart;
    public bool Cleared;

    // Taille en tuiles, murs compris. Toujours impaire pour avoir une porte centrée.
    public Vector2Int Size = new Vector2Int(13, 7);

    // Directions où il y a une porte (vers une salle voisine)
    public List<Vector2Int> Doors = new List<Vector2Int>();

    public RoomNode(Vector2Int pos) { GridPos = pos; }
}

public class Floor
{
    public int LevelIndex;               // 0 à 6
    public float DifficultyMultiplier;   // à utiliser pour PV / dégâts / nb d'ennemis
    public Dictionary<Vector2Int, RoomNode> Rooms = new Dictionary<Vector2Int, RoomNode>();
    public RoomNode StartRoom;
}

public static class FloorGenerator
{
    public const int LevelCount = 7;

    static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    /// <summary>Nombre de salles cible pour un niveau donné (0 à 6).</summary>
    public static int RoomCountForLevel(int level) => Mathf.Min(7 + level * 2, 22);

    /// <summary>Multiplicateur de difficulté : x1.0 au niveau 1, x2.5 au niveau 7.</summary>
    public static float DifficultyForLevel(int level) => 1f + level * 0.25f;

    public static Floor Generate(int level, int seed)
    {
        var rng = new System.Random(seed);
        int target = RoomCountForLevel(level);

        // On recommence tant que l'étage n'est pas valide (assez de salles + culs-de-sac)
        for (int attempt = 0; attempt < 100; attempt++)
        {
            var floor = TryGenerate(level, target, rng);
            if (floor != null) return floor;
        }

        Debug.LogError("Impossible de générer un étage valide.");
        return null;
    }

    static Floor TryGenerate(int level, int target, System.Random rng)
    {
        var floor = new Floor
        {
            LevelIndex = level,
            DifficultyMultiplier = DifficultyForLevel(level)
        };

        var start = new RoomNode(Vector2Int.zero) { Type = RoomType.Start };
        floor.Rooms[start.GridPos] = start;
        floor.StartRoom = start;

        var queue = new Queue<RoomNode>();
        queue.Enqueue(start);

        while (queue.Count > 0 && floor.Rooms.Count < target)
        {
            var current = queue.Dequeue();

            foreach (var dir in Shuffled(rng))
            {
                if (floor.Rooms.Count >= target) break;

                var pos = current.GridPos + dir;
                if (floor.Rooms.ContainsKey(pos)) continue;
                if (CountNeighbors(floor, pos) > 1) continue; // évite les boucles
                if (rng.NextDouble() < 0.4) continue;          // aléa : étage moins régulier

                var room = new RoomNode(pos) { DistanceFromStart = current.DistanceFromStart + 1 };
                floor.Rooms[pos] = room;
                Connect(current, room);
                queue.Enqueue(room);
            }

            // Si la file se vide trop tôt, on la relance pour continuer à étendre
            if (queue.Count == 0 && floor.Rooms.Count < target)
            {
                foreach (var r in floor.Rooms.Values) queue.Enqueue(r);
            }
        }

        if (floor.Rooms.Count < target) return null;

        if (!AssignSpecialRooms(floor)) return null;
        AssignSizes(floor, rng);
        return floor;
    }

    static bool AssignSpecialRooms(Floor floor)
    {
        // Culs-de-sac (1 seule porte), hors salle de départ, triés du plus loin au plus proche
        var deadEnds = new List<RoomNode>();
        foreach (var r in floor.Rooms.Values)
            if (r != floor.StartRoom && r.Doors.Count == 1) deadEnds.Add(r);

        if (deadEnds.Count < 3) return false; // boss + trésor + boutique

        deadEnds.Sort((a, b) => b.DistanceFromStart.CompareTo(a.DistanceFromStart));

        deadEnds[0].Type = RoomType.Boss;
        deadEnds[1].Type = RoomType.Treasure;
        deadEnds[2].Type = RoomType.Shop;
        return true;
    }

    static readonly Vector2Int[] NormalSizes =
    {
        new Vector2Int(13, 7),
        new Vector2Int(15, 9),
        new Vector2Int(17, 9),
        new Vector2Int(19, 11),
        new Vector2Int(21, 11)
    };

    static void AssignSizes(Floor floor, System.Random rng)
    {
        foreach (var room in floor.Rooms.Values)
        {
            switch (room.Type)
            {
                case RoomType.Start:
                    room.Size = new Vector2Int(13, 7);
                    break;
                case RoomType.Treasure:
                case RoomType.Shop:
                    room.Size = new Vector2Int(11, 7);
                    break;
                case RoomType.Boss:
                    room.Size = new Vector2Int(21, 13);
                    break;
                default:
                    room.Size = NormalSizes[rng.Next(NormalSizes.Length)];
                    break;
            }
        }
    }

    static void Connect(RoomNode a, RoomNode b)
    {
        a.Doors.Add(b.GridPos - a.GridPos);
        b.Doors.Add(a.GridPos - b.GridPos);
    }

    static int CountNeighbors(Floor floor, Vector2Int pos)
    {
        int count = 0;
        foreach (var d in Directions)
            if (floor.Rooms.ContainsKey(pos + d)) count++;
        return count;
    }

    static List<Vector2Int> Shuffled(System.Random rng)
    {
        var list = new List<Vector2Int>(Directions);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}