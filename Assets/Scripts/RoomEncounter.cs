using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// À ajouter sur une salle Normal. Quand le joueur entre :
/// verrouille les portes, fait apparaître les ennemis (budget selon niveau/difficulté/taille),
/// puis déverrouille quand ils sont tous morts.
/// </summary>
public class RoomEncounter : MonoBehaviour
{
    enum State { Waiting, Fighting, Cleared }

    // (type, coût dans le budget, poids de tirage)
    static readonly (EnemyKind kind, int cost, int weight)[] Table =
    {
        (EnemyKind.Chaser,  1, 4),
        (EnemyKind.Shooter, 2, 3),
        (EnemyKind.Charger, 2, 2),
    };

    const int MaxEnemies = 10;
    const float SpawnDelay = 0.7f;

    public event System.Action Cleared;
    public bool IsCleared => state == State.Cleared;

    Room room;
    int level;
    float difficulty;
    System.Random rng;
    State state = State.Waiting;
    Transform player;
    readonly List<EnemyBase> alive = new List<EnemyBase>();

    public void Init(Room room, int level, float difficulty, System.Random rng)
    {
        this.room = room;
        this.level = level;
        this.difficulty = difficulty;
        this.rng = rng;
    }

    void Update()
    {
        switch (state)
        {
            case State.Waiting:
                if (!FindPlayer()) return;
                Vector2 d = player.position - transform.position;
                // Le joueur doit être bien entré (au-delà de la zone des portes)
                if (Mathf.Abs(d.x) < room.Half.x - 1.5f && Mathf.Abs(d.y) < room.Half.y - 1.5f)
                    StartFight();
                break;

            case State.Fighting:
                alive.RemoveAll(e => e == null);
                if (alive.Count == 0) Clear();
                break;
        }
    }

    bool FindPlayer()
    {
        if (player != null) return true;
        var p = FindFirstObjectByType<PlayerController2D>();
        if (p == null) return false;
        player = p.transform;
        return true;
    }

    void StartFight()
    {
        state = State.Fighting;

        float area = (room.Half.x * 2 + 1) * (room.Half.y * 2 + 1);
        float sizeFactor = Mathf.Clamp(area / 150f, 0.6f, 1.5f);
        int budget = Mathf.Max(2, Mathf.RoundToInt((3f + level * 1.2f) * difficulty * sizeFactor));

        var spots = GetSpawnPositions();
        int i = 0;

        while (budget > 0 && i < spots.Count)
        {
            var pick = PickKind(budget);
            if (pick == null) break;

            alive.Add(EnemyFactory.Create(pick.Value.kind, spots[i++], transform, difficulty, SpawnDelay));
            budget -= pick.Value.cost;
        }

        if (alive.Count == 0) { Clear(); return; }
        SetDoorsLocked(true);
    }

    (EnemyKind kind, int cost, int weight)? PickKind(int budget)
    {
        int total = 0;
        foreach (var t in Table) if (t.cost <= budget) total += t.weight;
        if (total == 0) return null;

        int r = rng.Next(total);
        foreach (var t in Table)
        {
            if (t.cost > budget) continue;
            if (r < t.weight) return t;
            r -= t.weight;
        }
        return null;
    }

    List<Vector2> GetSpawnPositions()
    {
        var tiles = room.Tiles;
        int w = tiles.GetLength(0), h = tiles.GetLength(1);
        Vector2 origin = transform.position;
        Vector2 playerLocal = (Vector2)player.position - origin;

        var candidates = new List<Vector2>();
        for (int gx = 2; gx < w - 2; gx++)
            for (int gy = 2; gy < h - 2; gy++)
            {
                var def = tiles[gx, gy];
                if (def == null || def.solid) continue;

                Vector2 local = new Vector2(gx - room.Half.x, gy - room.Half.y);
                if (Vector2.Distance(local, playerLocal) < 6f) continue;

                bool nearDoor = false;
                foreach (var door in room.Doors)
                {
                    var dp = new Vector2(door.Direction.x * room.Half.x, door.Direction.y * room.Half.y);
                    if (Vector2.Distance(local, dp) < 4f) { nearDoor = true; break; }
                }
                if (nearDoor) continue;

                // Évite les rochers / props avec collider
                if (Physics2D.OverlapCircle(origin + local, 0.55f) != null) continue;

                candidates.Add(origin + local);
            }

        // Mélange (Fisher-Yates) avec le rng de la salle
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        // Espacement minimal entre ennemis
        var result = new List<Vector2>();
        foreach (var c in candidates)
        {
            bool ok = true;
            foreach (var r in result)
                if (Vector2.Distance(c, r) < 1.8f) { ok = false; break; }
            if (!ok) continue;

            result.Add(c);
            if (result.Count >= MaxEnemies) break;
        }
        return result;
    }

    void Clear()
    {
        state = State.Cleared;
        SetDoorsLocked(false);
        Cleared?.Invoke(); // point d'accroche : récompense, musique, etc.
    }

    void SetDoorsLocked(bool locked)
    {
        foreach (var door in room.Doors)
            if (door != null) door.SetLocked(locked);
    }
}
