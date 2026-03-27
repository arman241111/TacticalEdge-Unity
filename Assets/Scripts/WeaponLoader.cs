using UnityEngine;

public class WeaponLoader : MonoBehaviour
{
    private bool loaded = false;
    private GameObject currentGun = null;

    void Update()
    {
        if (!loaded && TeamSelect.teamSelected)
        {
            LoadWeapon();
        }
    }

    void LoadWeapon()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Find the right weapon based on team
        string teamWeapon = TeamSelect.playerTeam == "ct" ? "usp" : "glock";
        GameObject gun = null;

        var allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            string name = obj.name.ToLower();
            if (name.Contains(teamWeapon) || name.Contains("hk"))
            {
                if (obj.GetComponent<Camera>() == null && obj.GetComponent<CharacterController>() == null)
                {
                    gun = obj.gameObject;
                    break;
                }
            }
        }

        // Fallback - find any gun
        if (gun == null)
        {
            foreach (var obj in allObjects)
            {
                string name = obj.name.ToLower();
                if (name.Contains("glock") || name.Contains("(gen4)") || name.Contains("pistol") || name.Contains("usp") || name.Contains("hk") || name.Contains("gun"))
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
            // Hide the other weapon
            HideOtherWeapons(gun);

            // Parent to camera
            gun.transform.SetParent(cam.transform);

            // Auto-scale based on model size
            var rend = gun.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                float maxSize = Mathf.Max(rend.bounds.size.x, rend.bounds.size.y, rend.bounds.size.z);
                float targetSize = 0.4f;
                float scale = targetSize / maxSize;
                gun.transform.localScale = Vector3.one * scale;
            }
            else
            {
                gun.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
            gun.transform.localPosition = new Vector3(0.25f, -0.15f, 0.4f);
            gun.transform.localRotation = Quaternion.Euler(0, 0, 0);
            currentGun = gun;
            loaded = true;
            Debug.Log("Weapon attached: " + gun.name + " (team: " + TeamSelect.playerTeam + ")");
        }
    }

    void HideOtherWeapons(GameObject keepGun)
    {
        // Hide all other weapon models in scene
        var allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.gameObject == keepGun) continue;
            string name = obj.name.ToLower();
            if (name.Contains("glock") || name.Contains("(gen4)") || name.Contains("usp") || name.Contains("hk"))
            {
                if (obj.GetComponent<Camera>() == null && obj.GetComponent<CharacterController>() == null)
                {
                    obj.gameObject.SetActive(false);
                }
            }
        }
    }
}
