using UnityEngine;

public interface IDamageable { void TakeDamage(float amount); }

public abstract class TileEffect : ScriptableObject
{
    public virtual bool IsSafeGround => true;
    public virtual void OnEnter(TileAgent a) { }
    public virtual void OnStay(TileAgent a, float dt) { }
    public virtual void OnExit(TileAgent a) { }
}

[CreateAssetMenu(menuName = "Dungeon/Effects/Slow")]
public class SlowEffect : TileEffect          // boue
{
    [Range(0.05f, 1f)] public float multiplier = 0.5f;
    public override void OnStay(TileAgent a, float dt) =>
        a.SpeedMultiplier = Mathf.Min(a.SpeedMultiplier, multiplier);
}

[CreateAssetMenu(menuName = "Dungeon/Effects/Damage Over Time")]
public class DamageOverTimeEffect : TileEffect   // lave
{
    public float damagePerSecond = 20f;
    public override void OnStay(TileAgent a, float dt) => a.Damage(damagePerSecond * dt);
}

[CreateAssetMenu(menuName = "Dungeon/Effects/Spikes")]
public class SpikesEffect : TileEffect        // pics qui sortent en cycle
{
    public float damage = 1f, period = 2f, activeTime = 0.8f;
    public override void OnStay(TileAgent a, float dt)
    {
        if (Time.time % period < activeTime) a.Damage(damage); // compte sur les i-frames du joueur
    }
}

[CreateAssetMenu(menuName = "Dungeon/Effects/Pit")]
public class PitEffect : TileEffect           // crevasse
{
    public float fallDamage = 1f;
    public override bool IsSafeGround => false;
    public override void OnEnter(TileAgent a) => a.Fall(fallDamage);
}