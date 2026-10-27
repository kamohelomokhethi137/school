using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 3.2f;
    public float height = 1.55f;
    public float mouseSensitivity = 2.2f;
    public float minPitch = -30f;
    public float maxPitch = 65f;
    public float smooth = 12f;

    private float yaw;
    private float pitch = 12f;

    private void Start()
    {
        if (target == null)
        {
            PlayerController pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) target = pc.transform;
        }
        if (target != null) yaw = target.eulerAngles.y;
        LockCursor(true);
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) LockCursor(false);

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            LockCursor(true);

        if (Cursor.lockState != CursorLockMode.Locked || mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue();
        yaw += delta.x * mouseSensitivity * 0.1f;
        pitch -= delta.y * mouseSensitivity * 0.1f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * height;
        Vector3 desired = focus - rot * Vector3.forward * distance;

        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        Vector3 look = focus - transform.position;
        if (look.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(look.normalized);
    }

    private static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
