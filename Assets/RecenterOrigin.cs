using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections; // XROrigin

public class RecenterOrigin : MonoBehaviour
{
    [Header("Arah target dunia")]
    [Tooltip("Optional: jika diisi, forward Transform ini jadi arah target.")]
    public Transform referenceForward;
    public bool invertTargetForward = false;

    [Header("Posisi (opsional)")]
    public bool alsoReposition = false;
    public Vector2 targetXZ = Vector2.zero;
    public bool forceUpright = false; // set true agar X/Z rot = 0

    XROrigin xrOrigin;
    OVRCameraRig ovrRig;
    Vector3 targetForwardWorld; // flat, normalized, fixed

    void Awake()
    {
        xrOrigin = FindObjectOfType<XROrigin>(true);
        ovrRig = FindObjectOfType<OVRCameraRig>(true);

        // cache arah target sekali
        Vector3 baseFwd;
        if (referenceForward) baseFwd = Flat(referenceForward.forward);
        else if (xrOrigin)
        {
            GameObject originGO = xrOrigin.Origin.gameObject;
            baseFwd = Flat(originGO.transform.forward);
        }
        else baseFwd = Vector3.forward;

        targetForwardWorld = invertTargetForward ? -baseFwd : baseFwd;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Recenter();
    }

    [ContextMenu("Recenter")]
    public void Recenter()
    {
        bool xriOk = RecenterXR();   // jalan bahkan kalau XR rig non-aktif
        bool ovrOk = RecenterOVR();  // jalan bahkan kalau OVR rig non-aktif
        Debug.Log($"Recenter done. XRI={xriOk}, OVR={ovrOk}");
    }

    public void RecenterXROrigin()
    {
        StartCoroutine(WaitActive());
    }

    private IEnumerator WaitActive()
    {
        yield return new WaitForSeconds(0.1f);
        RecenterXR();
        Debug.Log($"Recenter done");
    }


    bool RecenterXR()
    {
        if (!xrOrigin) xrOrigin = FindObjectOfType<XROrigin>(true);
        if (!xrOrigin) return false;

        // cari kamera walau child non-aktif
        Transform cam = xrOrigin.Camera ? xrOrigin.Camera.transform
                                        : xrOrigin.GetComponentInChildren<Camera>(true)?.transform;
        if (!cam) return false;

        Transform originTx = xrOrigin.Origin.transform;

        Vector3 camFwd = Flat(cam.forward);
        float ang = Vector3.SignedAngle(camFwd, targetForwardWorld, Vector3.up);
        originTx.RotateAround(cam.position, Vector3.up, ang);

        if (forceUpright)
        {
            var e = originTx.eulerAngles;
            originTx.rotation = Quaternion.Euler(0f, e.y, 0f);
        }

        if (alsoReposition)
        {
            Vector3 camPos = cam.position;
            Vector3 dst = new Vector3(targetXZ.x, camPos.y, targetXZ.y);
            originTx.position += (dst - camPos);
        }
        return true;
    }

    bool RecenterOVR()
    {
        if (!ovrRig) ovrRig = FindObjectOfType<OVRCameraRig>(true);
        if (!ovrRig || !ovrRig.centerEyeAnchor) return false;

        Transform cam = ovrRig.centerEyeAnchor;
        Vector3 camFwd = Flat(cam.forward);
        float ang = Vector3.SignedAngle(camFwd, targetForwardWorld, Vector3.up);
        ovrRig.transform.RotateAround(cam.position, Vector3.up, ang);

        if (forceUpright)
        {
            var e = ovrRig.transform.eulerAngles;
            ovrRig.transform.rotation = Quaternion.Euler(0f, e.y, 0f);
        }

        if (alsoReposition)
        {
            Vector3 camPos = cam.position;
            Vector3 dst = new Vector3(targetXZ.x, camPos.y, targetXZ.y);
            ovrRig.transform.position += (dst - camPos);
        }
        return true;
    }

    static Vector3 Flat(Vector3 v)
    {
        var p = Vector3.ProjectOnPlane(v, Vector3.up);
        return p.sqrMagnitude < 1e-6f ? Vector3.forward : p.normalized;
    }
}
