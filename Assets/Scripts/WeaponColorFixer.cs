using UnityEngine;

public class WeaponColorFixer : MonoBehaviour
{
    void Start()
    {
        // Wait one frame for WeaponLoader to attach gun first
        Invoke("FixColors", 0.1f);
    }

    void FixColors()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Find all renderers in camera children (the weapon)
        var renderers = cam.GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                // If material is white/default, color it dark like a real Glock
                if (mat.color.r > 0.8f && mat.color.g > 0.8f && mat.color.b > 0.8f)
                {
                    mat.color = new Color(0.15f, 0.15f, 0.15f); // Dark gun metal
                    mat.SetFloat("_Metallic", 0.3f);
                    mat.SetFloat("_Smoothness", 0.4f);
                }
            }
        }
        Debug.Log("Weapon colors fixed!");
    }
}
