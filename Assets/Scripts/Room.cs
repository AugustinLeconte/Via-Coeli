using System.Collections.Generic;
using UnityEngine;

/// <summary>Représente une salle instanciée dans la scène.</summary>
public class Room : MonoBehaviour
{
    public RoomNode Node;
    public readonly List<RoomDoor> Doors = new List<RoomDoor>();

    public void SetDoorsLocked(bool locked)
    {
        foreach (var d in Doors) d.SetLocked(locked);
    }
}