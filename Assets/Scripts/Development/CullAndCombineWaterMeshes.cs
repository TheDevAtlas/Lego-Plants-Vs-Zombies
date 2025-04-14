using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class CullAndCombineWaterMeshes : MonoBehaviour
{
    public string targetLayerName = "Water";
    public string targetSaveName = "StaticEnvironmentMesh";
    public bool destroyOriginalsImmediately = true;

    private List<GameObject> originalObjects = new List<GameObject>();
    private GameObject combinedObject;

    void Start()
{
    Camera cam = Camera.main;
    if (cam == null)
    {
        Debug.LogError("Main Camera not found!");
        return;
    }

    int targetLayer = LayerMask.NameToLayer(targetLayerName);
    if (targetLayer == -1)
    {
        Debug.LogError($"Layer '{targetLayerName}' not found!");
        return;
    }

    MeshFilter[] allMeshFilters = FindObjectsOfType<MeshFilter>();
    Dictionary<Material, List<CombineInstance>> materialToCombines = new Dictionary<Material, List<CombineInstance>>();
    HashSet<GameObject> processedObjects = new HashSet<GameObject>();

    foreach (MeshFilter mf in allMeshFilters)
    {
        GameObject obj = mf.gameObject;
        if (obj.layer != targetLayer || mf.sharedMesh == null)
            continue;

        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (mr == null || mr.sharedMaterials.Length == 0)
            continue;

        Mesh mesh = mf.sharedMesh;
        Vector3[] verts = mesh.vertices;

        for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
        {
            int[] tris = mesh.GetTriangles(subMesh);
            List<int> newTris = new List<int>();

            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 v0 = obj.transform.TransformPoint(verts[tris[i]]);
                Vector3 v1 = obj.transform.TransformPoint(verts[tris[i + 1]]);
                Vector3 v2 = obj.transform.TransformPoint(verts[tris[i + 2]]);

                Vector3 faceNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                Vector3 toCamera = (cam.transform.position - v0).normalized;

                if (Vector3.Dot(faceNormal, toCamera) > 0f)
                {
                    newTris.Add(tris[i]);
                    newTris.Add(tris[i + 1]);
                    newTris.Add(tris[i + 2]);
                }
            }

            if (newTris.Count == 0)
                continue;

            Mesh culledMesh = new Mesh();
            culledMesh.vertices = verts;
            culledMesh.normals = mesh.normals;
            culledMesh.uv = mesh.uv;
            culledMesh.triangles = newTris.ToArray();

            CombineInstance ci = new CombineInstance
            {
                mesh = culledMesh,
                transform = obj.transform.localToWorldMatrix
            };

            Material mat = mr.sharedMaterials[Mathf.Min(subMesh, mr.sharedMaterials.Length - 1)];
            if (!materialToCombines.ContainsKey(mat))
                materialToCombines[mat] = new List<CombineInstance>();

            materialToCombines[mat].Add(ci);
        }

        processedObjects.Add(obj);
    }

    if (materialToCombines.Count == 0)
    {
        Debug.LogWarning("No eligible water meshes found to combine.");
        return;
    }

    GameObject root = new GameObject("StaticEnvironmentRoot");
    int index = 0;

    foreach (var kvp in materialToCombines)
    {
        Mesh subMesh = new Mesh();
        subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        subMesh.CombineMeshes(kvp.Value.ToArray(), true, true);
        SimplifyMesh(subMesh);

#if UNITY_EDITOR
        string folderPath = "Assets/Environment";
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "Environment");

        string meshAssetPath = $"{folderPath}/StaticPartMesh_{index}{targetSaveName}.asset";
        AssetDatabase.CreateAsset(Object.Instantiate(subMesh), meshAssetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"🗂️ Saved mesh: {meshAssetPath}");

        // Re-load to link it to the prefab
        subMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);
#endif

        GameObject part = new GameObject($"StaticPart_{index}");
        part.transform.parent = root.transform;

        MeshFilter mf = part.AddComponent<MeshFilter>();
        mf.sharedMesh = subMesh;

        MeshRenderer mr = part.AddComponent<MeshRenderer>();
        mr.sharedMaterial = kvp.Key;

        index++;
    }

    if (destroyOriginalsImmediately)
    {
        foreach (GameObject obj in processedObjects)
        {
            if (obj != null) Destroy(obj);
        }

        Debug.Log("🧹 Original objects destroyed immediately after combining.");
    }
    else
    {
        root.AddComponent<DestroyTracker>().Init(new List<GameObject>(processedObjects));
        Debug.Log("🧹 Original objects will be destroyed when parent is destroyed.");
    }

#if UNITY_EDITOR
    SaveAsPrefab(root);
#endif
}

    #if UNITY_EDITOR
    void SaveAsPrefab(GameObject obj)
    {
        string folderPath = "Assets/Environment";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Environment");
        }

        // Save the mesh as a separate asset
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            string meshPath = $"{folderPath}/{targetSaveName}.asset";
            Mesh meshToSave = Object.Instantiate(meshFilter.sharedMesh);
            AssetDatabase.CreateAsset(meshToSave, meshPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"🗂️ Mesh saved at: {meshPath}");

            // Reassign the saved mesh to ensure prefab links to it
            meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        }

        // Save the prefab
        string prefabPath = $"{folderPath}/{targetSaveName}Generated.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(obj, prefabPath, InteractionMode.AutomatedAction);
        Debug.Log($"📦 Prefab saved at: {prefabPath}");
    }
    #endif

    void SimplifyMesh(Mesh mesh)
    {
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize(); // Internal reorder to improve rendering performance
        mesh.UploadMeshData(false); // Upload and make it non-readable = false so we can still edit/save

        // Optional: Remove unused data (like tangents/colors if unused)
        mesh.tangents = null;
        mesh.colors = null;
    }



    class DestroyTracker : MonoBehaviour
    {
        private List<GameObject> toDestroy;

        public void Init(List<GameObject> originals)
        {
            toDestroy = new List<GameObject>(originals);
        }

        void OnDestroy()
        {
            foreach (GameObject obj in toDestroy)
            {
                if (obj != null)
                    Destroy(obj);
            }

            Debug.Log("🧹 Cleaned up original objects after destroying combined mesh.");
        }
    }
}
