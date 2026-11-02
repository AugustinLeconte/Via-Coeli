using System.Collections;
using UnityEngine;

/// <summary>Flash rouge sur le sprite du joueur quand il prend un coup. À ajouter sur le Player.</summary>
[RequireComponent(typeof(PlayerHealth))]
public class PlayerHurtFlash : MonoBehaviour
{
    [SerializeField] Color flashColor = new Color(1f, 0.25f, 0.25f);
    [SerializeField] float flashDuration = 0.15f;
    [Tooltip("Fait aussi clignoter le sprite pendant l'invincibilité")]
    [SerializeField] bool blinkDuringInvincibility = true;

    PlayerHealth health;
    SpriteRenderer sr;
    Color baseColor;
    Coroutine routine;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        sr = GetComponentInChildren<SpriteRenderer>();
        baseColor = sr.color;
    }

    void OnEnable()  => health.Hurt += OnHurt;
    void OnDisable()
    {
        health.Hurt -= OnHurt;
        if (sr != null) sr.color = baseColor;
    }

    void OnHurt(Vector2 hitDirection)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        // Fondu : rouge vif -> couleur normale
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(flashColor, baseColor, t / flashDuration);
            yield return null;
        }
        sr.color = baseColor;

        // Léger clignotement pendant le reste de l'invincibilité
        if (blinkDuringInvincibility)
        {
            float remaining = Mathf.Max(0f, health.invincibilityTime - flashDuration);
            float elapsed = 0f;
            while (elapsed < remaining)
            {
                elapsed += 0.08f;
                var c = baseColor; c.a = c.a > 0.5f ? 0.5f : 1f;
                sr.color = c;
                yield return new WaitForSeconds(0.08f);
            }
            sr.color = baseColor;
        }

        routine = null;
    }
}