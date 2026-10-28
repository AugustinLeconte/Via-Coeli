using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    [Tooltip("Invincibilité après un coup (secondes). Ne s'applique pas à la lave, qui inflige des dégâts continus.")]
    public float invincibilityTime = 0.5f;

    public float Current { get; private set; }
    float invincibleUntil;

    void Awake() => Current = maxHealth;

    public void TakeDamage(float amount)
    {
        if (Current <= 0f) return;

        bool isTick = amount < 1f;
        if (!isTick)
        {
            if (Time.time < invincibleUntil) return;
            invincibleUntil = Time.time + invincibilityTime;
        }

        Current = Mathf.Max(0f, Current - amount);
        Debug.Log($"Joueur : -{amount:0.00} PV ({Current:0.0}/{maxHealth})");
        if (Current <= 0f) Debug.Log("Joueur mort !");
    }
}