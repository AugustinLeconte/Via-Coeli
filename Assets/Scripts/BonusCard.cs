using UnityEngine;

public enum BonusType
{
    Damage, FireRate, ProjectileSpeed, Range, MaxHealth, Heal
}

[CreateAssetMenu(menuName = "Shop/Bonus Card", fileName = "NewBonusCard")]
public class BonusCard : ScriptableObject
{
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public Color color = Color.white;
    public int price = 10;

    [Header("Effet")]
    public BonusType type;
    [Tooltip("Valeur ajoutée à la stat (ex : +0.5 dégâts, +1 cœur, +1 de soin)")]
    public float value = 1f;

    public void Apply(GameObject player)
    {
        var shooter = player.GetComponentInParent<PlayerShooter>();
        var health  = player.GetComponentInParent<PlayerHealth>();

        switch (type)
        {
            case BonusType.Damage:          if (shooter) shooter.damage += value; break;
            case BonusType.FireRate:        if (shooter) shooter.fireRate += value; break;
            case BonusType.ProjectileSpeed: if (shooter) shooter.projectileSpeed += value; break;
            case BonusType.Range:           if (shooter) shooter.range += value; break;
            case BonusType.MaxHealth:
                if (health)
                {
                    health.SetMaxHealth(health.maxHealth + value);
                    health.Heal(value);   // on soigne du montant gagné
                }
                break;
            case BonusType.Heal:            if (health) health.Heal(value); break;
        }
    }
}