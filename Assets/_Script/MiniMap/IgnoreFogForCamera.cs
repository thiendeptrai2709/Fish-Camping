using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class IgnoreFogForCamera : MonoBehaviour
{
    private bool defaultFogState;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        RenderPipelineManager.endCameraRendering += OnEndCamera;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        RenderPipelineManager.endCameraRendering -= OnEndCamera;
    }

    private void OnBeginCamera(ScriptableRenderContext context, Camera camera)
    {
        if (camera == cam)
        {
            defaultFogState = RenderSettings.fog;
            RenderSettings.fog = false;
        }
    }

    private void OnEndCamera(ScriptableRenderContext context, Camera camera)
    {
        if (camera == cam)
        {
            RenderSettings.fog = defaultFogState;
        }
    }
}