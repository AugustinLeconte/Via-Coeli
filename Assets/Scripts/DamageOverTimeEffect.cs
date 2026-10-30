using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Effects/Damage Over Time")]
public class DamageOverTimeEffect : TileEffect
{
    public float damagePerTick = 0.5f;
    public float tickInterval = 1f;

    private readonly Dictionary<TileAgent, float> timers = new();

    private void OnEnable() => timers.Clear();

    public override void OnEnter(TileAgent a)
    {
        a.Damage(damagePerTick);   // dégât immédiat en entrant
        timers[a] = 0f;
    }

    public override void OnStay(TileAgent a, float dt)
    {
        if (!timers.TryGetValue(a, out float t))
        {
            // Sécurité : OnStay sans OnEnter (agent né sur la tuile, par ex.)
            a.Damage(damagePerTick);
            t = 0f;
        }

        t += dt;
        while (t >= tickInterval)
        {
            a.Damage(damagePerTick);
            t -= tickInterval;
        }

        timers[a] = t;
    }

    public override void OnExit(TileAgent a) => timers.Remove(a);
}