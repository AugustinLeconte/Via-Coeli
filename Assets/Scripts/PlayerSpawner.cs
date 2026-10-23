using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform startRoom;
    [Tooltip("Nombre de frames d'attente pour laisser le générateur finir.")]
    [SerializeField] private int waitFrames = 3;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindFirstObjectByType<PlayerSpawner>() != null) return;
        new GameObject("PlayerSpawner").AddComponent<PlayerSpawner>();
    }

    private IEnumerator Start()
    {
        for (int i = 0; i < waitFrames; i++) yield return null;

        var player = FindFirstObjectByType<PlayerController2D>();
        if (player == null) yield break;

        Transform room = startRoom != null ? startRoom : FindStartRoom();
        player.TeleportTo(room != null ? RoomDoor.GetRoomCenter(room) : Vector2.zero);
    }

    private Transform FindStartRoom()
    {
        RoomDoor best = null;
        float bestDist = float.MaxValue;
        foreach (var d in RoomDoor.All)
        {
            float dist = d.transform.position.sqrMagnitude;
            if (dist < bestDist) { bestDist = dist; best = d; }
        }
        return best != null ? best.transform.parent : null;
    }
}
