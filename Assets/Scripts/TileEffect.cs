using UnityEngine;

public abstract class TileEffect : ScriptableObject
{
    public virtual bool IsSafeGround => true;
    public virtual void OnEnter(TileAgent a) { }
    public virtual void OnStay(TileAgent a, float dt) { }
    public virtual void OnExit(TileAgent a) { }
}