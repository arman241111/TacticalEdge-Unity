using UnityEngine;

public class CharacterSetup : MonoBehaviour
{
    void Start()
    {
        // Scale up the character
        // Don't change scale - use default model size

        // Try to load and apply the main texture
        var renderers = GetComponentsInChildren<Renderer>();

        Texture2D mainTex = null;
        string[] searchPaths = {
            "Models/Mercenary/texture_pbr_20250901",
            "texture_pbr_20250901"
        };
        foreach (string path in searchPaths)
        {
            mainTex = Resources.Load<Texture2D>(path);
            if (mainTex != null) break;
        }

        foreach (var rend in renderers)
        {
            for (int i = 0; i < rend.materials.Length; i++)
            {
                var mat = rend.materials[i];
                if (mainTex != null)
                {
                    mat.mainTexture = mainTex;
                }
                else
                {
                    mat.color = new Color(0.3f, 0.32f, 0.28f);
                    mat.SetFloat("_Metallic", 0.1f);
                    mat.SetFloat("_Smoothness", 0.3f);
                }
            }
        }

        // Remove weapons from the model
        RemoveWeapons(transform);
    }

    void RemoveWeapons(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            string name = child.name.ToLower();

            if (name.Contains("gun") || name.Contains("rifle") || name.Contains("weapon") ||
                name.Contains("pistol") || name.Contains("knife") || name.Contains("holster_weapon"))
            {
                Debug.Log("Removed weapon part: " + child.name);
                Destroy(child.gameObject);
            }
            else
            {
                RemoveWeapons(child);
            }
        }
    }
}
