using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss : court vers le joueur, canalise puis charge quand il est à portée.
/// Toutes les N charges, il tire une salve circulaire de projectiles.
/// Reste inactif tant que le joueur n'est pas dans sa salle, et affiche une barre de vie à ce moment-là.
/// </summary>
public class BossEnemy : EnemyBase
{
    enum State { Run, Windup, Dash, Recover, BurstWindup }

    [Header("Charge")]
    public float chargeRange = 6f;       // distance à partir de laquelle il canalise
    public float windupTime = 0.8f;      // temps de canalisation
    public float dashSpeed = 16f;
    public float dashTime = 0.6f;
    public float recoverTime = 1f;

    [Header("Interface")]
    public string bossName = "Le Colosse";

    public bool PlayerInRoom => playerInRoom;
    public event System.Action Defeated;

    protected override void OnDeath() => Defeated?.Invoke();

    [Header("Salve de projectiles")]
    public int chargesPerBurst = 5;      // une salve toutes les N charges
    public int projectileCount = 16;
    public float burstWindupTime = 0.6f; // télégraphie avant la salve
    public float projectileSpeed = 6f;
    public float projectileDamage = 1f;

    [Header("Salle / barre de vie")]
    [Tooltip("Demi-taille de la zone où le joueur déclenche le boss (centrée sur son point d'apparition).")]
    public Vector2 roomHalfSize = new Vector2(9.5f, 5.5f);

    State state = State.Run;
    float timer;
    int chargeCount;
    float burstAngleOffset;
    Vector2 dashDir;
    Vector2 roomCenter;
    bool playerInRoom;

    GameObject barRoot;
    Image barFill;

    protected override Color DisplayColor
    {
        get
        {
            if (state == State.Windup) return Color.Lerp(baseColor, Color.red, 0.8f);
            if (state == State.BurstWindup) return Color.Lerp(baseColor, Color.yellow, 0.8f);
            return baseColor;
        }
    }

    void Start()
    {
        roomCenter = transform.position;
        BuildHealthBar();
    }

    protected override void Think()
    {
        // Inactif tant que le joueur n'est pas dans la salle
        Vector2 d = (Vector2)target.position - roomCenter;
        playerInRoom = Mathf.Abs(d.x) <= roomHalfSize.x && Mathf.Abs(d.y) <= roomHalfSize.y;
        if (!playerInRoom) { Stop(); return; }

        switch (state)
        {
            case State.Run:
                rb.linearVelocity = DirToTarget() * moveSpeed;
                if (DistToTarget() <= chargeRange)
                {
                    state = State.Windup;
                    timer = windupTime;
                }
                break;

            case State.Windup:
                Stop();
                dashDir = DirToTarget(); // vise pendant la canalisation, puis direction figée
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f) { state = State.Dash; timer = dashTime; }
                break;

            case State.Dash:
                rb.linearVelocity = dashDir * dashSpeed;
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f) EndDash();
                break;

            case State.Recover:
                Stop();
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f)
                {
                    if (chargesPerBurst > 0 && chargeCount % chargesPerBurst == 0)
                    {
                        state = State.BurstWindup;
                        timer = burstWindupTime;
                    }
                    else state = State.Run;
                }
                break;

            case State.BurstWindup:
                Stop();
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f)
                {
                    FireBurst();
                    state = State.Run;
                }
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        // La charge s'arrête sur un mur, un rocher ou le joueur
        if (state == State.Dash && c.collider.GetComponentInParent<EnemyBase>() == null)
            EndDash();
    }

    void EndDash()
    {
        chargeCount++;
        state = State.Recover;
        timer = recoverTime;
        Stop();
    }

    void FireBurst()
    {
        int count = Mathf.Max(1, projectileCount);
        float step = 360f / count;

        // Décale la salve d'un demi-pas une fois sur deux pour varier les trous
        burstAngleOffset = burstAngleOffset == 0f ? step * 0.5f : 0f;

        for (int i = 0; i < count; i++)
        {
            float a = (burstAngleOffset + i * step) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            Projectile.Spawn(transform.position, dir, projectileSpeed, projectileDamage, transform.parent);
        }
    }

    // ---------- Barre de vie ----------

    void BuildHealthBar()
    {
        barRoot = new GameObject("BossHealthBar");
        var canvas = barRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = barRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Nom
        var label = new GameObject("Name", typeof(RectTransform));
        label.transform.SetParent(barRoot.transform, false);
        var txt = label.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = bossName.ToUpper();
        txt.fontSize = 28;
        txt.alignment = TextAnchor.LowerCenter;
        txt.color = Color.white;
        label.AddComponent<Outline>().effectColor = Color.black;
        var lRt = txt.rectTransform;
        lRt.anchorMin = lRt.anchorMax = new Vector2(0.5f, 0f);
        lRt.pivot = new Vector2(0.5f, 0f);
        lRt.anchoredPosition = new Vector2(0f, 76f);
        lRt.sizeDelta = new Vector2(700f, 40f);

        // Barre
        var bg = NewImage("Background", barRoot.transform, new Color(0f, 0f, 0f, 0.75f));
        var bgRt = bg.rectTransform;
        bgRt.anchorMin = bgRt.anchorMax = new Vector2(0.5f, 0f);
        bgRt.pivot = new Vector2(0.5f, 0f);
        bgRt.anchoredPosition = new Vector2(0f, 40f);
        bgRt.sizeDelta = new Vector2(700f, 30f);

        barFill = NewImage("Fill", bg.transform, new Color(0.85f, 0.15f, 0.15f));
        var fRt = barFill.rectTransform;
        fRt.anchorMin = Vector2.zero;
        fRt.anchorMax = Vector2.one;
        fRt.offsetMin = new Vector2(4f, 4f);
        fRt.offsetMax = new Vector2(-4f, -4f);
        barFill.type = Image.Type.Filled;
        barFill.fillMethod = Image.FillMethod.Horizontal;
        barFill.fillOrigin = (int)Image.OriginHorizontal.Left;

        barRoot.SetActive(false);
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = EnemyFactory.Square;
        img.color = color;
        return img;
    }

    void LateUpdate()
    {
        if (barRoot == null) return;
        barRoot.SetActive(playerInRoom && !IsDead);
        if (barFill != null)
            barFill.fillAmount = Mathf.Clamp01(Health / maxHealth);
    }

    void OnDestroy()
    {
        if (barRoot != null) Destroy(barRoot);
    }
}