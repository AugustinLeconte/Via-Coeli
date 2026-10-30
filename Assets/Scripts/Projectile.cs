using UnityEngine;

/// <summary>Projectile ennemi : touche le joueur ou disparaît sur un mur / obstacle.</summary>
public class Projectile : MonoBehaviour
{
    float damage;
    Vector2 dir;

    public static Projectile Spawn(Vector2 pos, Vector2 direction, float speed, float damage, Transform parent)
    {
        var go = new GameObject("EnemyProjectile");
        go.transform.SetParent(parent, true);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.3f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = EnemyFactory.Square;
        sr.color = new Color(0.9f, 0.4f, 1f);
        sr.sortingOrder = 4;

        var body = go.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.linearVelocity = direction.normalized * speed; // avant Unity 6 : body.velocity

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var p = go.AddComponent<Projectile>();
        p.damage = damage;
        p.dir = direction.normalized;

        Destroy(go, 6f);
        return p;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (other.GetComponentInParent<EnemyBase>() != null) return; // pas de tir allié

        EnemyBase.TryDamagePlayer(other, damage, dir);
        Destroy(gameObject); // joueur, mur, rocher, porte...
    }
}
