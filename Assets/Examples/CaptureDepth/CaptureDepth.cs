using UnityEngine;
using System.Collections;

public class CaptureDepth : MonoBehaviour {
    public const string PROP_BLEND = "_Blend";

    public float blend = 0f;
    public Material depthView;

    Camera _attachedCamera;

    void OnEnable() {
        _attachedCamera = GetComponent<Camera> ();
    }
    void Update() {
        _attachedCamera.depthTextureMode = DepthTextureMode.Depth;
        blend = Mathf.Clamp01 (blend);
    }
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        depthView.SetFloat (PROP_BLEND, blend);
        Graphics.Blit (src, dst, depthView);
    }
}
