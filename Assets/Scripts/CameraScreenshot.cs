using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraScreenshot : MonoBehaviour
{
    public int resolutionWidth = 512;
    public int resolutionHeight = 512;
    public string savePath = "Assets/Screenshot.png";

    [Header("Transparency Settings")]
    public Color colorToMakeTransparent = Color.red;
    [Range(0f, 1f)]
    public float tolerance = 0.1f;
}

[CustomEditor(typeof(CameraScreenshot))]
public class CameraScreenshotEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("📸 Take Screenshot"))
        {
            CameraScreenshot script = (CameraScreenshot)target;
            TakeScreenshot(script);
        }
    }

    void TakeScreenshot(CameraScreenshot script)
    {
        Camera cam = script.GetComponent<Camera>();

        // Set up render texture
        RenderTexture rt = new RenderTexture(script.resolutionWidth, script.resolutionHeight, 24);
        cam.targetTexture = rt;

        // Backup camera settings
        Color originalBG = cam.backgroundColor;
        CameraClearFlags originalFlags = cam.clearFlags;

        // Use selected background color
        cam.backgroundColor = script.colorToMakeTransparent;
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Render to texture
        RenderTexture.active = rt;
        cam.Render();

        // Read pixels
        Texture2D tex = new Texture2D(script.resolutionWidth, script.resolutionHeight, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, script.resolutionWidth, script.resolutionHeight), 0, 0);
        tex.Apply();

        // Cleanup
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        // Convert background color to transparent with tolerance
        Color32[] pixels = tex.GetPixels32();
        Color target = script.colorToMakeTransparent;
        float tol = script.tolerance;

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            if (Mathf.Abs(p.r / 255f - target.r) < tol &&
                Mathf.Abs(p.g / 255f - target.g) < tol &&
                Mathf.Abs(p.b / 255f - target.b) < tol)
            {
                pixels[i] = new Color32(0, 0, 0, 0); // Transparent
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        // Save to PNG
        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(script.savePath, pngData);
        Debug.Log("📸 Screenshot saved to: " + script.savePath);

        // Refresh so it shows in Unity
        AssetDatabase.Refresh();
    }
}
