using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteAlways]
public class DepthScreenshotter : MonoBehaviour
{
    public int resolution = 1000;
    public Camera mainCamera;
    private Camera depthCamera;

    private string colorPath => Application.dataPath + "/ColorImage.png";
    private string depthPath => Application.dataPath + "/DepthImage.png";
    private string maskedPath => Application.dataPath + "/MaskedImage.png";

    [ContextMenu("Capture Color + Depth + Masked")]
    public void CaptureScreenshot()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found.");
            return;
        }

        // === COLOR ===
        RenderTexture colorRT = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
        mainCamera.targetTexture = colorRT;
        mainCamera.Render();

        Texture2D colorTex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        RenderTexture.active = colorRT;
        colorTex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        colorTex.Apply();
        File.WriteAllBytes(colorPath, colorTex.EncodeToPNG());
        Debug.Log($"Saved Color to: {colorPath}");

        // === DEPTH ===
        RenderTexture depthRT = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);

        if (depthCamera == null)
        {
            GameObject go = new GameObject("DepthCam");
            go.hideFlags = HideFlags.HideAndDontSave;
            depthCamera = go.AddComponent<Camera>();
            depthCamera.enabled = false;
        }

        depthCamera.CopyFrom(mainCamera);
        depthCamera.clearFlags = CameraClearFlags.SolidColor;
        depthCamera.backgroundColor = Color.white;
        depthCamera.targetTexture = depthRT;

        Shader depthShader = Shader.Find("Custom/ReplacementDepth");
        depthCamera.RenderWithShader(depthShader, "");

        Texture2D depthTex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        RenderTexture.active = depthRT;
        depthTex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        depthTex.Apply();
        File.WriteAllBytes(depthPath, depthTex.EncodeToPNG());
        Debug.Log($"Saved Depth to: {depthPath}");

        // === MASKED ===
        Texture2D maskedTex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color[] colorPixels = colorTex.GetPixels();
        Color[] depthPixels = depthTex.GetPixels();
        Color[] output = new Color[colorPixels.Length];

        for (int i = 0; i < colorPixels.Length; i++)
        {
            Color depthColor = depthPixels[i];
            bool isWhite = depthColor.r >= 0.999f && depthColor.g >= 0.999f && depthColor.b >= 0.999f;

            float alpha = isWhite ? 0f : 1f;
            Color color = colorPixels[i];
            output[i] = new Color(color.r, color.g, color.b, alpha);
        }


        maskedTex.SetPixels(output);
        maskedTex.Apply();
        File.WriteAllBytes(maskedPath, maskedTex.EncodeToPNG());
        Debug.Log($"Saved Masked image to: {maskedPath}");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        RenderTexture.active = null;
        mainCamera.targetTexture = null;
        depthCamera.targetTexture = null;
    }
}
