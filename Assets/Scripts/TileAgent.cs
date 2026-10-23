using UnityEngine;

[DefaultExecutionOrder(-10)]
public class TileAgent : MonoBehaviour
{
    public Vector2 feetOffset = new Vector2(0f, -0.3f);
    [Tooltip("Un ennemi volant ignore le sol.")]
    public bool flying;

    /// <summary>À multiplier dans le déplacement.</summary>
    public float SpeedMultiplier { get; set; } = 1f;
    public Vector2 LastSafePosition { get; private set; }

    TileDefinition current;

    void Start() => LastSafePosition = transform.position;

    void Update()
    {
        SpeedMultiplier = 1f;

        Vector2 feet = (Vector2)transform.position + feetOffset;
        TileDefinition tile = null;
        if (!flying)
        {
            var room = Room.FindAt(feet);
            if (room != null) tile = room.GetTileAt(feet);
        }

        if (tile != current)
        {
            if (current != null) foreach (var e in current.effects) e.OnExit(this);
            current = tile;
            if (current != null) foreach (var e in current.effects) e.OnEnter(this);
        }

        bool safe = true;
        if (current != null)
            foreach (var e in current.effects)
            {
                e.OnStay(this, Time.deltaTime);
                safe &= e.IsSafeGround;
            }
        if (safe) LastSafePosition = transform.position;
    }

    public void Damage(float amount)
    {
        if (TryGetComponent(out IDamageable d)) d.TakeDamage(amount);
    }

    public void Fall(float damage)
    {
        Damage(damage);
        if (TryGetComponent(out PlayerController2D p)) p.TeleportTo(LastSafePosition);
        else transform.position = LastSafePosition;
    }
}