using UnityEngine;

public class SeatedPoseFit : MonoBehaviour
{
    public PlayerController player;
    public float sink = 0.10f;
    public float soleToAnkle = 0.1544f;
    public float controllerHover = 0.08f;
    public float footForward = 0.26f;
    public float blendSpeed = 8f;

    private Transform hipLeft;
    private Transform hipRight;
    private Transform kneeLeft;
    private Transform kneeRight;
    private Transform footLeft;
    private Transform footRight;
    private Transform toeLeft;
    private Transform toeRight;
    private Vector3 baseLocalPosition;
    private float blend;

    void Awake()
    {
        baseLocalPosition = transform.localPosition;
        Transform[] all = GetComponentsInChildren<Transform>(true);
        hipLeft = FindBone(all, "mixamorig:LeftUpLeg");
        hipRight = FindBone(all, "mixamorig:RightUpLeg");
        kneeLeft = FindBone(all, "mixamorig:LeftLeg");
        kneeRight = FindBone(all, "mixamorig:RightLeg");
        footLeft = FindBone(all, "mixamorig:LeftFoot");
        footRight = FindBone(all, "mixamorig:RightFoot");
        toeLeft = FindBone(all, "mixamorig:LeftToeBase");
        toeRight = FindBone(all, "mixamorig:RightToeBase");
        if (player == null)
        {
            player = GetComponentInParent<PlayerController>();
        }
    }

    private static Transform FindBone(Transform[] all, string boneName)
    {
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == boneName) return all[i];
        }
        return null;
    }

    void LateUpdate()
    {
        bool seated = player != null && player.IsSeated;
        blend = Mathf.MoveTowards(blend, seated ? 1f : 0f, blendSpeed * Time.deltaTime);
        transform.localPosition = baseLocalPosition + Vector3.down * (sink * blend);

        if (blend <= 0.001f) return;

        float floorY = (player != null ? player.transform.position.y : 0f) - controllerHover;
        float targetAnkleY = floorY + soleToAnkle;
        SolveLeg(hipLeft, kneeLeft, footLeft, toeLeft, targetAnkleY);
        SolveLeg(hipRight, kneeRight, footRight, toeRight, targetAnkleY);
    }

    private void SolveLeg(Transform hip, Transform knee, Transform foot, Transform toe, float targetAnkleY)
    {
        if (hip == null || knee == null || foot == null) return;

        Vector3 hipPos = hip.position;
        Vector3 kneePos = knee.position;
        Vector3 anklePos = foot.position;

        float thighLen = (kneePos - hipPos).magnitude;
        float shinLen = (anklePos - kneePos).magnitude;
        if (thighLen < 0.001f || shinLen < 0.001f) return;

        Vector3 target = hipPos + transform.forward * footForward;
        target.x = anklePos.x;
        target.y = targetAnkleY;

        Vector3 toTarget = target - hipPos;
        float reach = toTarget.magnitude;
        if (reach < 0.0001f) return;

        float maxReach = thighLen + shinLen - 0.001f;
        if (reach > maxReach)
        {
            reach = maxReach;
            toTarget = toTarget.normalized * reach;
            target = hipPos + toTarget;
        }

        Vector3 axis = toTarget / reach;

        Vector3 kneeDir = kneePos - hipPos;
        Vector3 pole = kneeDir - axis * Vector3.Dot(kneeDir, axis);
        pole = pole.sqrMagnitude > 0.000001f ? pole.normalized : transform.forward;

        float along = (thighLen * thighLen - shinLen * shinLen + reach * reach) / (2f * reach);
        float height = Mathf.Sqrt(Mathf.Max(0f, thighLen * thighLen - along * along));
        Vector3 newKnee = hipPos + axis * along + pole * height;
        Vector3 newAnkle = hipPos + axis * reach;

        Quaternion startFootRotation = foot.rotation;

        Quaternion hipStep = Quaternion.Slerp(Quaternion.identity,
            Quaternion.FromToRotation(kneeDir, newKnee - hipPos), blend);
        hip.rotation = hipStep * hip.rotation;

        Vector3 kneeNow = knee.position;
        Quaternion kneeStep = Quaternion.Slerp(Quaternion.identity,
            Quaternion.FromToRotation(foot.position - kneeNow, newAnkle - kneeNow), blend);
        knee.rotation = kneeStep * knee.rotation;
        foot.rotation = Quaternion.Slerp(foot.rotation, startFootRotation, blend);

        if (toe == null) return;

        Vector3 footDir = toe.position - foot.position;
        Vector3 flatDir = new Vector3(footDir.x, 0f, footDir.z);
        if (flatDir.sqrMagnitude < 0.000001f) return;

        float pitch = Vector3.SignedAngle(flatDir, footDir, Vector3.right);
        foot.rotation = Quaternion.AngleAxis(-pitch * blend, Vector3.right) * foot.rotation;
    }
}
