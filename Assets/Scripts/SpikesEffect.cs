using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Effects/Spikes")]
public class SpikesEffect : TileEffect        // pics qui sortent en cycle
{
    public float damage = 1f, period = 2f, activeTime = 0.8f;
    public override void OnStay(TileAgent a, float dt)
    {
        if (Time.time % period < activeTime) a.Damage(damage); // compte sur les i-frames du joueur
    }
}