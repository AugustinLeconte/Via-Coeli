using UnityEngine;

/// <summary>Projectile du joueur : blesse les ennemis, disparaît sur les murs et obstacles.</summary>
public class PlayerProjectile : MonoBehaviour
{
    float damage;
    Vector2 dir;

    public static PlayerProjectile Spawn(Vector2 pos, Vector2 direction, float speed, float damage, float lifetime)
    {
        var go = new GameObject("PlayerProjectile");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.3f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = EnemyFactory.Square;              // remplace par ton vrai sprite plus tard
        sr.color = new Color(1f, 0.95f, 0.5f);
        sr.sortingOrder = 4;

        var body = go.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.linearVelocity = direction.normalized * speed; // avant Unity 6 : body.velocity

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var p = go.AddComponent<PlayerProjectile>();
        p.damage = damage;
        p.dir = direction.normalized;

        Destroy(go, lifetime);
        return p;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (other.GetComponentInParent<PlayerController2D>() != null) return; // pas sur soi-même

        var enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            if (!enemy.IsReady) return;               // ennemi en train d'apparaître : le tir passe à travers
            enemy.TakeDamage(damage, dir);
        }

        Destroy(gameObject); // ennemi touché, mur, rocher, porte...
    }
}