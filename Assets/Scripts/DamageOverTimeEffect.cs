using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Effects/Damage Over Time")]
public class DamageOverTimeEffect : TileEffect   // lave
{
    public float damagePerSecond = 20f;
    public override void OnStay(TileAgent a, float dt) => a.Damage(damagePerSecond * dt);
}