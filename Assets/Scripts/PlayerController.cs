using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform visual;
    public Transform cameraTransform;
    public float moveSpeed = 3.2f;
    public float turnSpeed = 12f;
    public float jumpForce = 4.5f;
    public float gravity = -18f;
    public float interactRange = 2.4f;

    private CharacterController controller;
    private Animator animator;
    private float verticalVelocity;
    private bool seated;
    private Interactable current;

    public bool IsSeated { get { return seated; } }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (visual == null)
        {
            visual = transform.Find("Visual");
        }
        animator = GetComponentInChildren<Animator>(true);
        SetAnimatorState(0f, false);
    }

    private void SetAnimatorState(float speed, bool isSeated)
    {
        if (animator == null) return;
        animator.SetFloat("Speed", speed);
        animator.SetBool("Seated", isSeated);
    }

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (seated)
        {
            current = null;
            SetAnimatorState(0f, true);
            if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            {
                StandUp();
            }
            return;
        }

        SetAnimatorState(0f, false);
        current = FindNearest();

        if (current != null && kb.eKey.wasPressedThisFrame && current.CanInteract())
        {
            current.Interact();
            return;
        }

        if (!controller.enabled) return;

        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;

        Vector3 fwd = Vector3.forward;
        Vector3 right = Vector3.right;
        if (cameraTransform != null)
        {
            Vector3 cf = cameraTransform.forward;
            cf.y = 0f;
            if (cf.sqrMagnitude > 0.001f) fwd = cf.normalized;
            Vector3 cr = cameraTransform.right;
            cr.y = 0f;
            if (cr.sqrMagnitude > 0.001f) right = cr.normalized;
        }

        Vector3 move = fwd * input.y + right * input.x;
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
            if (kb.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity = jumpForce;
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 motion = move * moveSpeed;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);

        Vector3 planarVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        SetAnimatorState(Mathf.Clamp01(planarVelocity.magnitude / moveSpeed), false);
    }

    private Interactable FindNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.up * 0.9f, interactRange);
        Interactable best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            Interactable it = hits[i].GetComponentInParent<Interactable>();
            if (it == null || !it.CanInteract()) continue;
            float d = (it.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = it;
            }
        }
        return best;
    }

    public void SitAt(Vector3 seatPosition, float seatYaw)
    {
        seated = true;
        controller.enabled = false;
        transform.position = seatPosition;
        transform.rotation = Quaternion.Euler(0f, seatYaw, 0f);
        verticalVelocity = 0f;
        SetAnimatorState(0f, true);
    }

    public void StandUp()
    {
        seated = false;
        SetAnimatorState(0f, false);
        transform.position += Vector3.up * 0.05f;
        controller.enabled = true;
        verticalVelocity = -2f;
    }

    void OnGUI()
    {
        string text = null;
        if (seated) text = "Press E to stand up";
        else if (current != null && current.CanInteract()) text = current.Prompt;
        if (string.IsNullOrEmpty(text)) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        float w = 360f;
        float h = 34f;
        Rect r = new Rect((Screen.width - w) * 0.5f, Screen.height * 0.60f, w, h);
        Color old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
        GUI.Label(r, text, style);
    }
}
