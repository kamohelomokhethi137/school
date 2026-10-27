using UnityEngine;

public class SitInteractable : Interactable
{
    public Transform sitAnchor;
    public float seatHeight = 0f;
    public float seatBackOffset = -0.1f;
    public float seatYaw = 180f;

    public override string Prompt
    {
        get
        {
            PlayerController pc = FindPlayer();
            if (pc != null && pc.IsSeated) return "Press E to stand up";
            return "Press E to sit down";
        }
    }

    public override void Interact()
    {
        PlayerController pc = FindPlayer();
        if (pc == null) return;

        if (pc.IsSeated)
        {
            pc.StandUp();
            return;
        }

        Transform a = sitAnchor != null ? sitAnchor : transform;
        Vector3 p = a.position;
        Vector3 seat = new Vector3(p.x, pc.transform.position.y + seatHeight, p.z + seatBackOffset);
        pc.SitAt(seat, seatYaw);
    }

    private static PlayerController FindPlayer()
    {
        return UnityEngine.Object.FindAnyObjectByType<PlayerController>();
    }
}
