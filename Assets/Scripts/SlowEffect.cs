using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Effects/Slow")]
public class SlowEffect : TileEffect
{
    [Range(0.05f, 1f)] public float multiplier = 0.5f;
    public override void OnStay(TileAgent a, float dt) =>
        a.SpeedMultiplier = Mathf.Min(a.SpeedMultiplier, multiplier);
}