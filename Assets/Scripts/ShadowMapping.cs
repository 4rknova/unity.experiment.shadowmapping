using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ShadowMapping : MonoBehaviour {
    public RenderTexture _target;
    public Light _light;
    Camera _shadowCam;
    Shader _depthShader;

    void Start()
    {
        _depthShader = Shader.Find("Hidden/CustomShadows/Depth");
        SetUpShadowCam();
    }

    void Update ()
    {
        UpdateShadowCameraPosition();
        UpdateShaderValues();
        _shadowCam.targetTexture = _target;
        _shadowCam.RenderWithShader(_depthShader, "");
    }

    void UpdateShaderValues()
    {

        // Set the qualities of the textures
        Shader.SetGlobalTexture("_ShadowTex", _target);
        Matrix4x4 lightMatrix = _shadowCam.nonJitteredProjectionMatrix * _shadowCam.worldToCameraMatrix;
        Shader.SetGlobalMatrix("_LightMatrix", lightMatrix);
        Vector4 lightDirection =  -_light.transform.forward;
        Shader.SetGlobalVector("_LightDirection", lightDirection);
    }

    void UpdateShadowCameraPosition()
    {
        Camera cam = _shadowCam;

        cam.transform.position = _light.transform.position;
        cam.transform.rotation = _light.transform.rotation;
        cam.transform.LookAt(cam.transform.position + cam.transform.forward, cam.transform.up);

        Vector3 center, extents;
        List<Renderer> renderers = new List<Renderer>();
        renderers.AddRange(FindObjectsOfType<Renderer>());
        GetRenderersExtents(renderers, cam.transform, out center, out extents);

        center.z -= extents.z / 2;
        cam.transform.position = cam.transform.TransformPoint(center);
        cam.nearClipPlane = 0;
        cam.farClipPlane = extents.z;

        cam.aspect = extents.x / extents.y;
        cam.orthographicSize = extents.y / 2;
    }

    void SetUpShadowCam()
    {
        if (_shadowCam) return;

        GameObject go = new GameObject("_shadowCam");
        go.hideFlags = HideFlags.DontSave;

        _shadowCam = go.AddComponent<Camera>();
        _shadowCam.orthographic = true;
        _shadowCam.nearClipPlane = 0;
        _shadowCam.enabled = false;
        _shadowCam.backgroundColor = new Color(0, 0, 0, 0);
        _shadowCam.clearFlags = CameraClearFlags.SolidColor;
    }

    void GetRenderersExtents(List<Renderer> renderers, Transform frame, out Vector3 center, out Vector3 extents)
    {
        Vector3[] arr = new Vector3[8];

        Vector3 min = Vector3.one * Mathf.Infinity;
        Vector3 max = Vector3.one * Mathf.NegativeInfinity;

        foreach (var r in renderers) {
            GetBoundsPoints(r.bounds, arr, frame.worldToLocalMatrix);

            foreach(var p in arr) {
                for(int i = 0; i < 3; i ++)
                {
                    min[i] = Mathf.Min(p[i], min[i]);
                    max[i] = Mathf.Max(p[i], max[i]);
                }
            }
        }

        extents = max - min;
        center = (max + min) / 2;
    }

    // Returns the 8 points for the given bounds multiplied by
    // the given matrix
    void GetBoundsPoints(Bounds b, Vector3[] points, Matrix4x4? mat = null)
    {
        Matrix4x4 trans = mat ?? Matrix4x4.identity;

        int count = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 v = b.extents;
                    v.x *= x;
                    v.y *= y;
                    v.z *= z;
                    v += b.center;
                    v = trans.MultiplyPoint(v);

                    points[count++] = v;
                }
    }

}
