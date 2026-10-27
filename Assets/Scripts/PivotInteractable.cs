using UnityEngine;

public class PivotInteractable : Interactable
{
    public Vector3 openEuler = new Vector3(0f, 90f, 0f);
    public float speed = 3f;
    public string openPrompt = "Press E to open";
    public string closePrompt = "Press E to close";

    private Quaternion closedRot;
    private Quaternion openRot;
    private bool isOpen;
    private bool animating;

    public override string Prompt
    {
        get { return isOpen ? closePrompt : openPrompt; }
    }

    void Awake()
    {
        closedRot = transform.localRotation;
        openRot = closedRot * Quaternion.Euler(openEuler);
    }

    public override void Interact()
    {
        isOpen = !isOpen;
        animating = true;
    }

    void Update()
    {
        if (!animating) return;

        Quaternion target = isOpen ? openRot : closedRot;
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            target,
            speed * 90f * Time.deltaTime);

        if (Quaternion.Angle(transform.localRotation, target) < 0.1f)
        {
            transform.localRotation = target;
            animating = false;
        }
    }
}
