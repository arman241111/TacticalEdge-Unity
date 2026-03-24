using UnityEngine;

public class WeaponLoader : MonoBehaviour
{
    void Start()
    {
        // Find the gun model in the scene
        // Look for any object with "Glock" or "glock" in name
        GameObject gun = null;

        // Search in all children of Main Camera
        Camera cam = Camera.main;
        if (cam == null) return;

        foreach (Transform child in cam.transform)
        {
            if (child.name.ToLower().Contains("glock") || child.name.ToLower().Contains("gun") || child.name.ToLower().Contains("pistol"))
            {
                gun = child.gameObject;
                break;
            }
        }

        // If not found as child, search entire scene
        if (gun == null)
        {
            var allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                string name = obj.name.ToLower();
                if (name.Contains("glock") || name.Contains("(gen4)") || name.Contains("pistol"))
                {
                    if (obj.GetComponent<Camera>() == null && obj.GetComponent<CharacterController>() == null)
                    {
                        gun = obj.gameObject;
                        break;
                    }
                }
            }
        }

        if (gun != null)
        {
            // Parent to camera
            gun.transform.SetParent(cam.transform);
            gun.transform.localPosition = new Vector3(0.45f, -0.15f, 0.9f);
            gun.transform.localRotation = Quaternion.Euler(0, 0, 0);
            gun.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            Debug.Log("Weapon attached to camera: " + gun.name);
        }
        else
        {
            Debug.LogWarning("No weapon model found in scene!");
        }
    }
}
