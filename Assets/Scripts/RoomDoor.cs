using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Une porte : bloque le passage quand elle est verrouillée, et téléporte le joueur
/// vers la porte voisine quand elle est ouverte. La liaison se fait toute seule.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class RoomDoor : MonoBehaviour
{
    public Vector2Int Direction;   // up / down / left / right

    public Color LockedColor = new Color(0.55f, 0.15f, 0.15f);
    public Color OpenColor = new Color(0.25f, 0.2f, 0.15f);

    [Tooltip("Rempli automatiquement. Peut être assigné à la main si besoin.")]
    public RoomDoor destination;

    [Tooltip("Où le joueur apparaît en arrivant par cette porte (enfant de la porte). Optionnel.")]
    public Transform spawnPoint;

    [SerializeField] private float maxLinkDistance = 100f;
    [SerializeField] private bool debugLogs = true;
    [Tooltip("Marge supplémentaire (en unités) entre la porte et le joueur à l'arrivée.")]
    [SerializeField] private float arrivalMargin = 0.2f;

    public bool IsLocked { get; private set; }

    /// <summary>Toutes les portes actives de la scène.</summary>
    public static readonly List<RoomDoor> All = new List<RoomDoor>();

    private SpriteRenderer sr;
    private BoxCollider2D col;       // bloque le passage quand verrouillée
    private BoxCollider2D trigger;   // détecte le joueur (toujours actif)
    private bool initialized;
    private string lastLog;

    private const float Cooldown = 0.3f;
    private static float lastUseTime = float.NegativeInfinity;

    // Remet les variables static à zéro à chaque lancement, même si
    // "Reload Domain" est désactivé dans Enter Play Mode Options.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        All.Clear();
        lastUseTime = float.NegativeInfinity;
    }

    private void Awake() => Init();
    private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    private void OnDisable() => All.Remove(this);

    // Start s'exécute après que le générateur a fini de configurer la porte (taille, sprite...).
    private void Start() => RefreshTrigger();

    private void Log(string msg)
    {
        if (!debugLogs || msg == lastLog) return; // évite le spam
        lastLog = msg;
        Debug.Log($"[RoomDoor] {name} : {msg}", this);
    }

    private void Init()
    {
        if (initialized) return;
        initialized = true;

        sr = GetComponent<SpriteRenderer>();

        // On retrouve les colliders existants (évite d'en créer en double après un rechargement de code).
        foreach (var c in GetComponents<BoxCollider2D>())
        {
            if (c.isTrigger) { if (trigger == null) trigger = c; }
            else if (col == null) col = c;
        }
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();

        IsLocked = col.enabled;

        // Le collider bloquant est désactivé quand la porte est ouverte :
        // on utilise donc un trigger séparé, un peu plus large, pour détecter le joueur.
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
        }
        RefreshTrigger();
    }

    private void RefreshTrigger()
    {
        Init();
        trigger.offset = col.offset;
        trigger.size = col.size + Vector2.one * 0.5f;
    }

    public void SetLocked(bool locked)
    {
        Init();
        IsLocked = locked;
        col.enabled = locked;
        sr.color = locked ? LockedColor : OpenColor;
    }

    /// <summary>Centre d'une salle : centre des sprites enfants, sinon position du parent.</summary>
    public static Vector2 GetRoomCenter(Transform room)
    {
        var renderers = room.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return room.position;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b.center;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryUse(other);
    private void OnTriggerStay2D(Collider2D other) => TryUse(other);

    private void TryUse(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        if (IsLocked) { Log("verrouillée, pas de téléportation."); return; }

        // since < 0 : valeur périmée d'une ancienne partie, on l'ignore.
        float since = Time.time - lastUseTime;
        if (since >= 0f && since < Cooldown) return;

        if (destination == null) destination = FindPartner();
        if (destination == null)
        {
            Log($"aucune porte partenaire trouvée (Direction={Direction}, portes dans la scène={All.Count}, parent={(transform.parent != null ? transform.parent.name : "AUCUN")}).");
            return;
        }

        lastUseTime = Time.time;
        Vector2 target = destination.GetArrivalPoint(other.bounds);

        Log($"téléportation vers {destination.name} en {target}.");
        player.TeleportTo(target);
    }

    /// <summary>
    /// Point d'arrivée du joueur quand il sort par cette porte : juste à l'intérieur de la salle,
    /// assez loin pour ne pas toucher le trigger de la porte (donc pas de re-téléportation).
    /// </summary>
    public Vector2 GetArrivalPoint(Bounds playerBounds)
    {
        if (spawnPoint != null) return spawnPoint.position;
        RefreshTrigger();

        // Géométrie du trigger calculée à la main : fiable même si la porte vient d'être créée
        // ou n'est pas encore active (Collider2D.bounds serait alors vide).
        Vector2 center = transform.TransformPoint(trigger.offset);
        Vector2 scale = new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        Vector2 extents = Vector2.Scale(trigger.size, scale) * 0.5f;

        // Vers l'intérieur de la salle : à l'opposé de la Direction de la porte,
        // ou à défaut vers le centre de la salle.
        Vector2 inward;
        if (Direction != Vector2Int.zero) inward = -(Vector2)Direction;
        else if (transform.parent != null) inward = GetRoomCenter(transform.parent) - center;
        else return center;

        bool horizontal = Mathf.Abs(inward.x) > Mathf.Abs(inward.y);
        inward = horizontal ? new Vector2(Mathf.Sign(inward.x), 0f) : new Vector2(0f, Mathf.Sign(inward.y));

        float doorExtent = horizontal ? extents.x : extents.y;
        float playerExtent = horizontal ? playerBounds.extents.x : playerBounds.extents.y;

        return center + inward * (doorExtent + playerExtent + arrivalMargin);
    }

    private RoomDoor FindPartner()
    {
        RoomDoor best = null;
        float bestDist = maxLinkDistance;

        foreach (var d in All)
        {
            if (d == this) continue;
            if (transform.parent != null && d.transform.parent == transform.parent) continue; // même salle
            if (d.destination != null && d.destination != this) continue;                     // déjà prise

            // Si les directions sont renseignées, on veut la porte qui lui fait face.
            if (Direction != Vector2Int.zero && d.Direction != Vector2Int.zero && d.Direction != -Direction) continue;

            float dist = Vector2.Distance(transform.position, d.transform.position);
            if (dist < bestDist) { bestDist = dist; best = d; }
        }

        if (best != null) best.destination = this; // lien dans les deux sens
        return best;
    }
}