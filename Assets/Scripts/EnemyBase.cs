using UnityEngine;

/// <summary>
/// Base commune : vie, dégâts, recul, flash, délai d'apparition, dégâts de contact.
/// Les sous-classes n'implémentent que Think() (appelé dans FixedUpdate).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 3f;
    public float moveSpeed = 3f;
    public float contactDamage = 1f;
    public float contactCooldown = 0.8f;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Transform target;
    protected Color baseColor = Color.white;
    protected float health;

    float spawnReadyTime, hitFlashTimer, knockbackTimer, nextContactTime;
    bool dead;

    /// <summary>False pendant l'apparition (invulnérable et inactif).</summary>
    public bool IsReady => !dead && Time.time >= spawnReadyTime;

    /// <summary>Couleur affichée hors flash de dégâts (les sous-classes peuvent la surcharger).</summary>
    protected virtual Color DisplayColor => baseColor;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Init(Color color, float hp, float speed, float difficulty, float spawnDelay)
    {
        baseColor = color;
        maxHealth = health = Mathf.Max(1f, Mathf.Round(hp * difficulty));
        moveSpeed = speed;
        spawnReadyTime = Time.time + spawnDelay;
    }

    void Update()
    {
        if (sr == null) return;

        if (!IsReady)
        {
            var c = baseColor; c.a = 0.35f;
            sr.color = c;
            return;
        }

        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
            sr.color = Color.white;
        }
        else sr.color = DisplayColor;
    }

    void FixedUpdate()
    {
        if (!IsReady) { rb.linearVelocity = Vector2.zero; return; } // avant Unity 6 : rb.velocity

        if (knockbackTimer > 0f) { knockbackTimer -= Time.fixedDeltaTime; return; }

        if (target == null)
        {
            var p = FindFirstObjectByType<PlayerController2D>();
            if (p == null) { Stop(); return; }
            target = p.transform;
        }

        Think();
    }

    /// <summary>Comportement propre à chaque ennemi.</summary>
    protected abstract void Think();

    // ---------- Helpers ----------
    protected Vector2 DirToTarget() => ((Vector2)target.position - (Vector2)transform.position).normalized;
    protected float DistToTarget() => Vector2.Distance(target.position, transform.position);
    protected void Stop() => rb.linearVelocity = Vector2.zero;

    // ---------- Dégâts ----------
    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (!IsReady) return;

        health -= amount;
        hitFlashTimer = 0.08f;

        if (health <= 0f) { Die(); return; }

        knockbackTimer = 0.12f;
        rb.linearVelocity = hitDirection.normalized * 6f;
        OnHurt();
    }

    protected virtual void OnHurt() { }
    protected virtual void OnDeath() { }

    void Die()
    {
        dead = true;
        OnDeath(); // ex : drop, particules
        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (!IsReady || contactDamage <= 0f || Time.time < nextContactTime) return;

        Vector2 dir = ((Vector2)c.collider.transform.position - (Vector2)transform.position).normalized;
        if (TryDamagePlayer(c.collider, contactDamage, dir))
            nextContactTime = Time.time + contactCooldown;
    }

    /// <summary>Inflige des dégâts si le collider appartient au joueur. Renvoie true si c'est le joueur.</summary>
    public static bool TryDamagePlayer(Collider2D col, float damage, Vector2 dir)
    {
        if (col.GetComponentInParent<PlayerController2D>() == null) return false;

        var dmg = col.GetComponentInParent<IDamageable>();
        if (dmg == null)
            Debug.LogWarning("Le joueur n'implémente pas IDamageable : aucun dégât appliqué.", col);
        else
            dmg.TakeDamage(damage, dir);
        return true;
    }
}
