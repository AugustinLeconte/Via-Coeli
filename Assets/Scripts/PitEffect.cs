using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Effects/Pit")]
public class PitEffect : TileEffect           // crevasse
{
    public float fallDamage = 1f;
    public override bool IsSafeGround => false;
    public override void OnEnter(TileAgent a) => a.Fall(fallDamage);
}