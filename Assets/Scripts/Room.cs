using System.Collections.Generic;
using UnityEngine;

/// <summary>Représente une salle instanciée dans la scène.</summary>
public class Room : MonoBehaviour
{
    public RoomNode Node;
    public readonly List<RoomDoor> Doors = new List<RoomDoor>();

public TileDefinition[,] Tiles;
public Vector2Int Half;

public static readonly List<Room> All = new List<Room>();

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void ResetStatics() => All.Clear();

void OnEnable()  { if (!All.Contains(this)) All.Add(this); }
void OnDisable() => All.Remove(this);

public TileDefinition GetTileAt(Vector2 world)
{
    Vector2 l = world - (Vector2)transform.position;
    int gx = Mathf.RoundToInt(l.x) + Half.x;
    int gy = Mathf.RoundToInt(l.y) + Half.y;
    if (Tiles == null || gx < 0 || gy < 0 || gx >= Tiles.GetLength(0) || gy >= Tiles.GetLength(1))
        return null;
    return Tiles[gx, gy];
}

    public static Room FindAt(Vector2 world)
    {
        foreach (var r in All) if (r.GetTileAt(world) != null) return r;
        return null;
    }

    public void SetDoorsLocked(bool locked)
    {
        foreach (var d in Doors) d.SetLocked(locked);
    }
}