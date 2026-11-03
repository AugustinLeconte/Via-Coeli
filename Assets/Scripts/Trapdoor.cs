using UnityEngine;

/// <summary>Trappe : le joueur la touche, on passe au niveau suivant.</summary>
public class Trapdoor : MonoBehaviour
{
    System.Action onEnter;
    bool used;

    public static Trapdoor Spawn(Vector2 pos, Transform parent, System.Action onEnter)
    {
        var go = new GameObject("Trapdoor");
        go.transform.SetParent(parent, true);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 1.6f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = EnemyFactory.Square;
        sr.color = new Color(0.25f, 0.18f, 0.1f);
        sr.sortingOrder = 1;

        // Intérieur sombre
        var inner = new GameObject("Inner");
        inner.transform.SetParent(go.transform, false);
        inner.transform.localScale = Vector3.one * 0.7f;
        var isr = inner.AddComponent<SpriteRenderer>();
        isr.sprite = EnemyFactory.Square;
        isr.color = new Color(0.03f, 0.02f, 0.05f);
        isr.sortingOrder = 2;

        go.AddComponent<BoxCollider2D>().isTrigger = true;

        var t = go.AddComponent<Trapdoor>();
        t.onEnter = onEnter;
        return t;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || other.GetComponentInParent<PlayerController2D>() == null) return;
        used = true;
        onEnter?.Invoke();
    }
}