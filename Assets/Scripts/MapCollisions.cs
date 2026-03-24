using UnityEngine;

public class MapCollisions : MonoBehaviour
{
    void Start()
    {
        // Don't change map scale - use default

        // Add mesh colliders to all mesh objects in the map
        AddColliders(transform);
        Debug.Log("Map collisions added and scaled!");
    }

    void AddColliders(Transform parent)
    {
        foreach (Transform child in parent)
        {
            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                // Check if collider already exists
                if (child.GetComponent<Collider>() == null)
                {
                    MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
            }

            // Recurse into children
            if (child.childCount > 0)
                AddColliders(child);
        }
    }
}
