using UnityEngine;

public class MapCollisions : MonoBehaviour
{
    void Start()
    {
        AddColliders(transform);
        Debug.Log("Map collisions added!");
    }

    void AddColliders(Transform parent)
    {
        // Add MeshCollider to every MeshFilter that doesn't have a collider
        MeshFilter mf = parent.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            if (parent.GetComponent<Collider>() == null)
            {
                MeshCollider mc = parent.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }
        }

        // Also check MeshRenderer without MeshFilter (skinned meshes etc)
        MeshRenderer mr = parent.GetComponent<MeshRenderer>();
        if (mr != null && parent.GetComponent<Collider>() == null)
        {
            // Try to add box collider as fallback
            BoxCollider bc = parent.gameObject.AddComponent<BoxCollider>();
            bc.size = mr.bounds.size;
            bc.center = parent.InverseTransformPoint(mr.bounds.center);
        }

        // Recurse into children
        for (int i = 0; i < parent.childCount; i++)
        {
            AddColliders(parent.GetChild(i));
        }
    }
}
