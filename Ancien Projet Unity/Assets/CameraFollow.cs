using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Cible")]
    [SerializeField] private Transform target;   // Le joueur

    [Header("Suivi")]
    [SerializeField] private bool smooth = false;
    [SerializeField] private float smoothSpeed = 8f;

    private float startZ;

    private void Awake()
    {
        // On garde le Z de la caméra (en 2D elle est en général à -10)
        startZ = transform.position.z;
    }

    // LateUpdate : la caméra bouge après que le joueur a fini de se déplacer
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, startZ);

        if (smooth)
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        else
            transform.position = desired;
    }
}
