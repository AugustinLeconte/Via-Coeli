using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;
    [Tooltip("Zoom : plus petit = plus zoomé. Valeur par défaut d'Unity : 5.")]
    [SerializeField] private float orthographicSize = 5f;

    private Vector3 velocity;

    private void Awake()
    {
        var cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;

        // Trouve le joueur tout seul si non assigné.
        if (target == null)
        {
            var player = FindFirstObjectByType<PlayerController2D>();
            if (player != null) target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);

        // Téléportation (changement de salle) : on colle la caméra directement.
        if (Vector2.Distance(transform.position, goal) > orthographicSize * 1.5f)
        {
            transform.position = goal;
            velocity = Vector3.zero;
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }
}