using System.Collections.Generic;
using UnityEngine;

public class FloorBuilder : MonoBehaviour
{
    [Range(0, 6)] public int level = 0;
    public int seed = 12345;
    public bool randomSeed = true;

    [Tooltip("Distance entre le centre de deux salles voisines. Doit être > à la plus grande salle (21x13).")]
    public Vector2 roomSpacing = new Vector2(30f, 20f);

    [Tooltip("Zoome la caméra pour voir tout l'étage (utile pour tester).")]
    public bool fitCameraToFloor = true;

    public Floor Floor { get; private set; }
    public readonly Dictionary<Vector2Int, Room> Rooms = new Dictionary<Vector2Int, Room>();

    void Start() => Build();

    [ContextMenu("Regenerate")]
    public void Build()
    {
        Clear();

        if (randomSeed) seed = Random.Range(0, int.MaxValue);
        Floor = FloorGenerator.Generate(level, seed);
        if (Floor == null) return;

        foreach (var node in Floor.Rooms.Values)
        {
            var worldPos = new Vector2(node.GridPos.x * roomSpacing.x,
                                       node.GridPos.y * roomSpacing.y);
            Rooms[node.GridPos] = RoomBuilder.Build(node, transform, worldPos);
        }

        Debug.Log($"Niveau {level + 1} : {Floor.Rooms.Count} salles, " +
                  $"difficulté x{Floor.DifficultyMultiplier:0.00} (seed {seed})");

        if (fitCameraToFloor) FitCamera();
    }

    void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        Rooms.Clear();
    }

    void FitCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
        foreach (var pos in Floor.Rooms.Keys)
        {
            min = Vector2Int.Min(min, pos);
            max = Vector2Int.Max(max, pos);
        }

        Vector2 center = new Vector2((min.x + max.x) * 0.5f * roomSpacing.x,
                                     (min.y + max.y) * 0.5f * roomSpacing.y);
        float totalW = (max.x - min.x + 1) * roomSpacing.x;
        float totalH = (max.y - min.y + 1) * roomSpacing.y;

        cam.orthographic = true;
        cam.transform.position = new Vector3(center.x, center.y, -10f);
        //cam.orthographicSize = Mathf.Max(totalH * 0.5f, totalW * 0.5f / cam.aspect);
    }
}
