using UnityEngine;

/// <summary>Distance : garde ses distances, tourne autour du joueur et tire des projectiles.</summary>
public class ShooterEnemy : EnemyBase
{
    [Header("Tir")]
    public float preferredMin = 4f;
    public float preferredMax = 7f;
    public float fireInterval = 1.8f;
    public float projectileSpeed = 6f;
    public float projectileDamage = 1f;
    public float maxFireRange = 11f;

    float fireTimer = 1f;
    float strafeSign = 1f;
    float strafeTimer = 2f;

    protected override void Think()
    {
        float dist = DistToTarget();
        Vector2 toPlayer = DirToTarget();
        Vector2 move;

        if (dist < preferredMin) move = -toPlayer;                    // trop près : recule
        else if (dist > preferredMax) move = toPlayer;                // trop loin : approche
        else move = Vector2.Perpendicular(toPlayer) * strafeSign * 0.6f; // à bonne distance : tourne autour

        rb.linearVelocity = move * moveSpeed;

        strafeTimer -= Time.fixedDeltaTime;
        if (strafeTimer <= 0f)
        {
            strafeSign = -strafeSign;
            strafeTimer = Random.Range(1.5f, 3f);
        }

        fireTimer -= Time.fixedDeltaTime;
        if (fireTimer <= 0f && dist <= maxFireRange)
        {
            Projectile.Spawn(transform.position, toPlayer, projectileSpeed, projectileDamage, transform.parent);
            fireTimer = fireInterval;
        }
    }
}
