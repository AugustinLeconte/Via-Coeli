using UnityEngine;

public enum EnemyKind { Chaser, Shooter, Charger }

/// <summary>
/// Crée les ennemis par code, sans prefab ni sprite (carrés colorés).
/// Plus tard : remplace Create par un Instantiate de prefabs.
/// </summary>
public static class EnemyFactory
{
    static Sprite square;

    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                square = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return square;
        }
    }

    public static EnemyBase Create(EnemyKind kind, Vector2 worldPos, Transform parent,
                                   float difficulty, float spawnDelay)
    {
        var go = new GameObject($"Enemy {kind}");
        go.transform.SetParent(parent, true);
        go.transform.position = worldPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square;
        sr.sortingOrder = 3;

        var body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        go.AddComponent<CircleCollider2D>().radius = 0.5f;

        EnemyBase enemy;
        Color color;
        float hp, speed, scale;

        switch (kind)
        {
            case EnemyKind.Shooter:
                enemy = go.AddComponent<ShooterEnemy>();
                color = new Color(0.6f, 0.35f, 0.8f); hp = 2f; speed = 2.5f; scale = 0.7f;
                break;
            case EnemyKind.Charger:
                enemy = go.AddComponent<ChargerEnemy>();
                color = new Color(0.9f, 0.6f, 0.2f); hp = 6f; speed = 2f; scale = 1.1f;
                break;
            default:
                enemy = go.AddComponent<ChaserEnemy>();
                color = new Color(0.85f, 0.3f, 0.25f); hp = 3f; speed = 3f; scale = 0.8f;
                break;
        }

        go.transform.localScale = Vector3.one * scale;
        enemy.Init(color, hp, speed, difficulty, spawnDelay);
        return enemy;
    }
}
