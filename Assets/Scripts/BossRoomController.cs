using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pilote la salle du boss :
/// Sealed (étage pas fini) -> Open -> Fight (portes verrouillées) -> Done (trappe).
/// </summary>
public class BossRoomController : MonoBehaviour
{
    enum Phase { Sealed, Open, Fight, Done }

    FloorBuilder builder;
    Room room;
    BossEnemy boss;
    Phase phase = Phase.Sealed;

    List<RoomEncounter> encounters;
    List<RoomDoor> approachDoors; // portes des salles voisines qui mènent au boss

    public void Init(FloorBuilder builder, Room room, BossEnemy boss)
    {
        this.builder = builder;
        this.room = room;
        this.boss = boss;

        // La zone de déclenchement suit la vraie taille de la salle (comme RoomEncounter)
        boss.roomHalfSize = new Vector2(room.Half.x - 1.5f, room.Half.y - 1.5f);
        boss.Defeated += OnBossDefeated;
        room.SetDoorsLocked(true);
    }

    void Update()
    {
        if (encounters == null) CacheRooms(); // toutes les salles existent à ce stade

        switch (phase)
        {
            case Phase.Sealed:
                if (AllOtherRoomsCleared())
                {
                    room.SetDoorsLocked(false);
                    SetApproachDoorsLocked(false);
                    phase = Phase.Open;
                }
                else
                {
                    // Ré-imposé à chaque frame : RoomEncounter déverrouille ses portes en fin de combat
                    room.SetDoorsLocked(true);
                    SetApproachDoorsLocked(true);
                }
                break;

            case Phase.Open:
                if (boss != null && boss.PlayerInRoom)
                {
                    room.SetDoorsLocked(true);
                    phase = Phase.Fight;
                }
                break;
        }
    }

    void CacheRooms()
    {
        encounters = new List<RoomEncounter>();
        approachDoors = new List<RoomDoor>();

        foreach (var r in builder.Rooms.Values)
        {
            if (r == room) continue;

            var e = r.GetComponent<RoomEncounter>();
            if (e != null) encounters.Add(e);

            // Une porte de la salle voisine qui pointe vers la salle du boss
            foreach (var d in r.Doors)
                if (r.Node.GridPos + d.Direction == room.Node.GridPos)
                    approachDoors.Add(d);
        }
    }

    bool AllOtherRoomsCleared()
    {
        foreach (var e in encounters)
            if (e != null && !e.IsCleared) return false;
        return true;
    }

    void SetApproachDoorsLocked(bool locked)
    {
        foreach (var d in approachDoors)
            if (d != null) d.SetLocked(locked);
    }

    void OnBossDefeated()
    {
        phase = Phase.Done;
        room.SetDoorsLocked(false);
        Trapdoor.Spawn(room.transform.position, room.transform, builder.NextLevel);
    }
}