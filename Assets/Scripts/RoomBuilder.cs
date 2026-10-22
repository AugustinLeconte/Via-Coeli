using UnityEngine;

/// <summary>
/// Construit une salle à partir d'un RoomNode, sans aucun asset :
/// les tuiles sont des carrés blancs de 1x1 unité, teintés par code.
/// Plus tard, remplace CreateTile par de vrais sprites / un Tilemap.
/// </summary>
public static class RoomBuilder
{
    static Sprite whiteSprite;

    static Sprite White
    {
        get
        {
            if (whiteSprite == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                // 1 pixel par unité => le sprite fait exactement 1x1 unité
                whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return whiteSprite;
        }
    }

    static readonly Color WallColor = new Color(0.2f, 0.2f, 0.25f);

    static Color FloorColor(RoomType type)
    {
        switch (type)
        {
            case RoomType.Start:    return new Color(0.35f, 0.55f, 0.35f);
            case RoomType.Boss:     return new Color(0.6f, 0.25f, 0.25f);
            case RoomType.Treasure: return new Color(0.7f, 0.6f, 0.25f);
            case RoomType.Shop:     return new Color(0.25f, 0.55f, 0.6f);
            default:                return new Color(0.45f, 0.4f, 0.35f);
        }
    }

    public static Room Build(RoomNode node, Transform parent, Vector2 worldPos)
    {
        var root = new GameObject($"Room {node.Type} {node.GridPos}");
        root.transform.SetParent(parent);
        root.transform.position = worldPos;

        var room = root.AddComponent<Room>();
        room.Node = node;

        int w = node.Size.x;
        int h = node.Size.y;
        int hx = (w - 1) / 2;   // demi-largeur en tuiles
        int hy = (h - 1) / 2;   // demi-hauteur en tuiles

        // Sol : un seul sprite étiré sur l'intérieur de la salle
        CreateTile("Floor", root.transform, Vector2.zero,
                   new Vector2(w - 2, h - 2), FloorColor(node.Type), 0);

        // Périmètre : murs, avec une porte là où il y a une connexion
        for (int x = -hx; x <= hx; x++)
        {
            for (int y = -hy; y <= hy; y++)
            {
                bool isPerimeter = Mathf.Abs(x) == hx || Mathf.Abs(y) == hy;
                if (!isPerimeter) continue;

                var pos = new Vector2(x, y);
                Vector2Int? doorDir = DoorDirectionAt(x, y, hx, hy);

                if (doorDir.HasValue && node.Doors.Contains(doorDir.Value))
                {
                    var go = CreateTile("RoomDoor", root.transform, pos, Vector2.one, Color.white, 2);
                    go.AddComponent<BoxCollider2D>();
                    var door = go.AddComponent<RoomDoor>();
                    door.Direction = doorDir.Value;
                    door.SetLocked(false);
                    room.Doors.Add(door);
                }
                else
                {
                    var wall = CreateTile("Wall", root.transform, pos, Vector2.one, WallColor, 1);
                    wall.AddComponent<BoxCollider2D>();
                }
            }
        }

        return room;
    }

    /// <summary>Si la case (x,y) du périmètre est au milieu d'un côté, renvoie la direction de la porte.</summary>
    static Vector2Int? DoorDirectionAt(int x, int y, int hx, int hy)
    {
        if (x == 0 && y == hy)  return Vector2Int.up;
        if (x == 0 && y == -hy) return Vector2Int.down;
        if (y == 0 && x == -hx) return Vector2Int.left;
        if (y == 0 && x == hx)  return Vector2Int.right;
        return null;
    }

    static GameObject CreateTile(string name, Transform parent, Vector2 localPos,
                                 Vector2 size, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = White;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }
}
