using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Tooltip("Nombre de cœurs max (5 = 10 demi-cœurs)")]
    public float maxHealth = 5;

    [Tooltip("Invincibilité après un coup (secondes). Ne s'applique pas à la lave, qui inflige des dégâts continus.")]
    public float invincibilityTime = 0.5f;

    [Header("UI")]
    [SerializeField] RectTransform heartsContainer;
    [SerializeField] Image heartPrefab;
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    public float Current { get; private set; }

    public event System.Action<Vector2> Hurt;

    float invincibleUntil;
    Image[] hearts;

    void Awake()
    {
        Current = maxHealth;
        BuildHearts();
        RefreshHearts();
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (Current <= 0f) return;

        bool isTick = amount < 0.5f;
        if (!isTick)
        {
            if (Time.time < invincibleUntil) return;
            invincibleUntil = Time.time + invincibilityTime;
        }

        Current = Mathf.Max(0f, Current - amount);
        RefreshHearts();

        if (!isTick) Hurt?.Invoke(hitDirection);   // pas de recul/anim pour les dégâts continus
        if (Current <= 0f) Debug.Log("Joueur mort !");
    }

    public void Heal(float amount)
    {
        if (Current <= 0f) return;
        Current = Mathf.Min(maxHealth, Current + amount);
        RefreshHearts();
    }

    public void SetMaxHealth(float newMax, bool healToFull = false)
    {
        maxHealth = newMax;
        Current = healToFull ? maxHealth : Mathf.Min(Current, maxHealth);
        BuildHearts();
        RefreshHearts();
    }

    void BuildHearts()
    {
        foreach (Transform child in heartsContainer)
            Destroy(child.gameObject);

        int count = Mathf.CeilToInt(maxHealth);
        hearts = new Image[count];
        for (int i = 0; i < count; i++)
            hearts[i] = Instantiate(heartPrefab, heartsContainer);
    }

    void RefreshHearts()
    {
        // Vie convertie en demi-cœurs (arrondi au-dessus : une fraction de vie compte comme une moitié)
        int halves = Mathf.CeilToInt(Current * 2f);

        for (int i = 0; i < hearts.Length; i++)
        {
            int halvesInThisHeart = halves - i * 2;   // combien de moitiés pour ce cœur

            if (halvesInThisHeart >= 2)      hearts[i].sprite = fullHeart;
            else if (halvesInThisHeart == 1) hearts[i].sprite = halfHeart;
            else                             hearts[i].sprite = emptyHeart;
        }
    }
}