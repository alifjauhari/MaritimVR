using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class LabelAboveTarget : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // set to Hats.001 (the mesh root)

    [Header("Positioning")]
    public float extraHeight = 0.08f;        // extra world units above the top
    public float followLerp = 20f;           // 0 = snap, higher = smoother

    [Header("Facing")]
    public bool faceCamera = true;           // billboard to camera
    public Camera cam;                       // leave null to use Camera.main

    Renderer[] _renderers;
    RectTransform _rt;

    void OnEnable()
    {
        _rt = GetComponent<RectTransform>();
        RefreshRenderers();
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (!target) return;
        if (_renderers == null || _renderers.Length == 0) RefreshRenderers();

        // World AABB of the whole target (works with MeshRenderer & SkinnedMeshRenderer)
        Bounds b = GetWorldBounds();

        // Top in world Y + padding
        Vector3 top = b.center + Vector3.up * (b.extents.y + extraHeight);

        // Smooth follow (works in edit & play)
        transform.position = Vector3.Lerp(transform.position, top, Time.deltaTime * followLerp);

        // Optional billboard so the UI is always readable
        if (faceCamera && (cam || (cam = Camera.main)))
        {
            Vector3 dir = (transform.position - cam.transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    void RefreshRenderers()
    {
        _renderers = target ? target.GetComponentsInChildren<Renderer>(true) : null;
    }

    Bounds GetWorldBounds()
    {
        if (_renderers == null || _renderers.Length == 0)
            return new Bounds(target.position, Vector3.one * 0.1f);

        Bounds b = new Bounds(_renderers[0].bounds.center, Vector3.zero);
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i].enabled) b.Encapsulate(_renderers[i].bounds);
        return b;
    }
}
