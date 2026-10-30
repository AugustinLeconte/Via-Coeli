using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Vector2 hitDirection);
}

public static class DamageableExtensions
{
    // Pour les sources sans direction (lave, poison...)
    public static void TakeDamage(this IDamageable d, float amount) => d.TakeDamage(amount, Vector2.zero);
}