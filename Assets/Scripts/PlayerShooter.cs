using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tir du joueur. Clavier/souris : vise à la souris, clic gauche (maintenu) pour tirer.
/// Manette : le stick droit vise et tire automatiquement.
/// À ajouter sur le même GameObject que PlayerController2D.
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 1f;
    [Tooltip("Tirs par seconde")]
    public float fireRate = 3f;
    public float projectileSpeed = 12f;
    [Tooltip("Portée en unités")]
    public float range = 9f;
    [Tooltip("Distance de spawn devant le joueur")]
    public float muzzleOffset = 0.6f;

    InputAction fireAction;
    InputAction aimStickAction;
    float nextFireTime;
    Camera cam;

    void Awake()
    {
        fireAction = new InputAction("Fire", InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");

        aimStickAction = new InputAction("AimStick", InputActionType.Value);
        aimStickAction.AddBinding("<Gamepad>/rightStick");
    }

    void OnEnable()
    {
        fireAction.Enable();
        aimStickAction.Enable();
    }

    void OnDisable()
    {
        fireAction.Disable();
        aimStickAction.Disable();
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused) return;
        if (Time.time < nextFireTime) return;

        Vector2 stick = aimStickAction.ReadValue<Vector2>();
        bool stickAiming = stick.sqrMagnitude > 0.3f * 0.3f;

        Vector2 dir;
        if (stickAiming)
        {
            dir = stick.normalized;
        }
        else if (fireAction.IsPressed())
        {
            if (!TryGetMouseDirection(out dir)) return;
        }
        else return;

        Shoot(dir);
    }

    bool TryGetMouseDirection(out Vector2 dir)
    {
        dir = Vector2.zero;
        if (cam == null) cam = Camera.main;
        if (cam == null || Mouse.current == null) return false;

        Vector3 mp = Mouse.current.position.ReadValue();
        mp.z = -cam.transform.position.z;               // distance caméra -> plan 2D
        Vector2 world = cam.ScreenToWorldPoint(mp);

        dir = world - (Vector2)transform.position;
        if (dir.sqrMagnitude < 0.01f) return false;
        dir.Normalize();
        return true;
    }

    void Shoot(Vector2 dir)
    {
        nextFireTime = Time.time + 1f / fireRate;

        Vector2 pos = (Vector2)transform.position + dir * muzzleOffset;
        PlayerProjectile.Spawn(pos, dir, projectileSpeed, damage, range / projectileSpeed);
    }
}