using UnityEngine;
using UnityEngine.Tilemaps;

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

    public static Room Build(RoomNode node, Transform parent, Vector2 worldPos,
                            RoomStyle style, int level, System.Random rng)
    {
        var root = new GameObject($"Room {node.Type} {node.GridPos}");
        root.transform.SetParent(parent);
        root.transform.position = worldPos;
        root.AddComponent<Grid>();

        var room = root.AddComponent<Room>();
        room.Node = node;

        var layout = RoomLayoutGenerator.Generate(node, style, level, rng);
        room.Tiles = layout.Tiles;
        room.Half = layout.Half;

        var ground = CreateTilemap("Ground", root.transform, 0, false);
        var walls  = CreateTilemap("Walls",  root.transform, 1, true);

        int w = layout.Tiles.GetLength(0), h = layout.Tiles.GetLength(1);
        for (int gx = 0; gx < w; gx++)
            for (int gy = 0; gy < h; gy++)
            {
                var def = layout.Tiles[gx, gy];
                
                
                
                if (def == null)
                {
                    Debug.LogError($"Tile null en ({gx},{gy}) dans {node.Type} {node.GridPos} (style : {style?.name})", style);
                    continue;
                }
                if (style.floor == null)
                {
                    Debug.LogError($"style.floor non assigné sur {style.name}", style);
                    continue;
                }
                
                
                
                var cell = new Vector3Int(gx - layout.Half.x, gy - layout.Half.y, 0);
                if (def.solid)
                {
                    ground.SetTile(cell, style.floor.Pick(rng));   // sol sous les murs/rochers
                    walls.SetTile(cell, def.Pick(rng));
                }
                else ground.SetTile(cell, def.Pick(rng));
            }

        // Portes : objets à part, comme avant
        foreach (var dir in node.Doors)
        {
            var pos = new Vector2(dir.x * layout.Half.x, dir.y * layout.Half.y);
            var go = CreateTile("RoomDoor", root.transform, pos, Vector2.one, Color.white, 2);
            go.AddComponent<BoxCollider2D>();
            var door = go.AddComponent<RoomDoor>();
            door.Direction = dir;
            door.LockedColor = style.doorLockedColor;
            door.OpenColor = style.doorOpenColor;
            door.SetLocked(false);
            room.Doors.Add(door);
        }

        PropSpawner.Spawn(room, layout, style, rng);
        return room;
    }

    static Tilemap CreateTilemap(string name, Transform parent, int order, bool collider)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        // Une cellule Tilemap (0,0) est dessinée en (0.5,0.5) : on décale pour centrer sur les entiers
        go.transform.localPosition = new Vector3(-0.5f, -0.5f, 0f);

        var tm = go.AddComponent<Tilemap>();
        go.AddComponent<TilemapRenderer>().sortingOrder = order;

        if (collider)
        {
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            var tc = go.AddComponent<TilemapCollider2D>();
            go.AddComponent<CompositeCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge; // avant Unity 2023 : tc.usedByComposite = true
        }
        return tm;
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
