using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract void Interact(GameObject who);
}

// Exemple : un levier qui déverrouille les portes de sa salle
public class Lever : Interactable
{
    bool used;
    public override void Interact(GameObject who)
    {
        if (used) return;
        used = true;
        GetComponentInParent<Room>()?.SetDoorsLocked(false);
    }
}