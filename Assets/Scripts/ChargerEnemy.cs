using UnityEngine;

/// <summary>
/// Brute : s'approche lentement, se prépare (rougit), puis fonce en ligne droite
/// jusqu'à toucher quelque chose, et reste vulnérable un instant.
/// </summary>
public class ChargerEnemy : EnemyBase
{
    enum State { Chase, Windup, Dash, Recover }

    [Header("Charge")]
    public float triggerRange = 7f;
    public float windupTime = 0.7f;
    public float dashSpeed = 13f;
    public float dashTime = 0.7f;
    public float recoverTime = 1f;

    State state = State.Chase;
    float timer;
    float cooldown = 1f;
    Vector2 dashDir;

    protected override Color DisplayColor =>
        state == State.Windup ? Color.Lerp(baseColor, Color.red, 0.8f) : baseColor;

    protected override void Think()
    {
        switch (state)
        {
            case State.Chase:
                rb.linearVelocity = DirToTarget() * moveSpeed;
                cooldown -= Time.fixedDeltaTime;
                if (cooldown <= 0f && DistToTarget() < triggerRange)
                {
                    state = State.Windup;
                    timer = windupTime;
                }
                break;

            case State.Windup:
                Stop();
                dashDir = DirToTarget(); // vise pendant la préparation, puis la direction est figée
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f) { state = State.Dash; timer = dashTime; }
                break;

            case State.Dash:
                rb.linearVelocity = dashDir * dashSpeed;
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f) EndDash();
                break;

            case State.Recover:
                Stop();
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f) { state = State.Chase; cooldown = 0.5f; }
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        // La charge s'arrête sur un mur, un rocher ou le joueur
        if (state == State.Dash && c.collider.GetComponentInParent<EnemyBase>() == null)
            EndDash();
    }

    void EndDash()
    {
        state = State.Recover;
        timer = recoverTime;
        Stop();
    }

    protected override void OnHurt()
    {
        // Un coup reçu pendant la préparation n'annule pas la charge, mais pendant la charge oui
        if (state == State.Dash) EndDash();
    }
}
