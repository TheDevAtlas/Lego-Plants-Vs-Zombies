using System.Collections.Generic;
using UnityEngine;

public class CullAndCombineWaterMeshes : MonoBehaviour
{
    public string targetLayerName = "Water";
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

        List<CombineInstance> finalCombines = new List<CombineInstance>();
        List<Material> finalMaterials = new List<Material>();

        foreach (var kvp in materialToCombines)
        {
            Mesh subMesh = new Mesh();
            subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            subMesh.CombineMeshes(kvp.Value.ToArray(), true, true);

            CombineInstance ci = new CombineInstance
            {
                mesh = subMesh,
                transform = Matrix4x4.identity
            };

            finalCombines.Add(ci);
            finalMaterials.Add(kvp.Key);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(finalCombines.ToArray(), false, false);

        combinedObject = new GameObject("CombinedWaterMesh");
        combinedObject.transform.position = Vector3.zero;

        MeshFilter mfCombined = combinedObject.AddComponent<MeshFilter>();
        mfCombined.sharedMesh = combinedMesh;

        MeshRenderer mrCombined = combinedObject.AddComponent<MeshRenderer>();
        mrCombined.sharedMaterials = finalMaterials.ToArray();

        List<GameObject> originals = new List<GameObject>(processedObjects);

        if (destroyOriginalsImmediately)
        {
            foreach (GameObject obj in originals)
            {
                if (obj != null) Destroy(obj);
            }

            Debug.Log("🧹 Original objects destroyed immediately after combining.");
        }
        else
        {
            combinedObject.AddComponent<DestroyTracker>().Init(originals);
            Debug.Log("🧹 Original objects will be destroyed when combined object is destroyed.");
        }

        Debug.Log($"✅ Combined {processedObjects.Count} objects using {finalMaterials.Count} unique material(s).");
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
