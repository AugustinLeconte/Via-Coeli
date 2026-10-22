using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    // Les bindings sont définis ici : pas besoin de créer un asset Input Actions.
    private InputAction moveAction;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick");
    }

    private void OnEnable() => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

    private void Update()
    {
        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
    }

    private void FixedUpdate()
    {
        // Sous Unity 2022 ou antérieur, remplacer linearVelocity par velocity.
        rb.linearVelocity = moveInput * moveSpeed;
    }

    /// <summary>Téléporte le joueur (utilisé par les portes).</summary>
    public void TeleportTo(Vector2 position)
    {
        rb.position = position;
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
    }
}
