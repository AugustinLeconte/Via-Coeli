using UnityEngine;

/// <summary>Mêlée basique : fonce sur le joueur et fait des dégâts au contact.</summary>
public class ChaserEnemy : EnemyBase
{
    protected override void Think()
    {
        rb.linearVelocity = DirToTarget() * moveSpeed;
    }
}
