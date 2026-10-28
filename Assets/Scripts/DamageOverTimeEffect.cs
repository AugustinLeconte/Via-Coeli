using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Effects/Damage Over Time")]
public class DamageOverTimeEffect : TileEffect
{
    public float damagePerSecond = 0.5f;
    public override void OnStay(TileAgent a, float dt) => a.Damage(damagePerSecond * dt);
}