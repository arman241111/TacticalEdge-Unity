using UnityEngine;

public class SimpleCrosshair : MonoBehaviour
{
    private Texture2D crosshairTex;
    private SimpleShoot gun;
    private PlayerHealth hp;

    void Start()
    {
        crosshairTex = new Texture2D(2, 2);
        crosshairTex.SetPixel(0, 0, Color.white);
        crosshairTex.SetPixel(1, 0, Color.white);
        crosshairTex.SetPixel(0, 1, Color.white);
        crosshairTex.SetPixel(1, 1, Color.white);
        crosshairTex.Apply();

        gun = GetComponent<SimpleShoot>();
        hp = GetComponent<PlayerHealth>();
    }

    void OnGUI()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        GUI.color = new Color(1, 1, 1, 0.9f);

        // Center dot
        GUI.DrawTexture(new Rect(cx - 2, cy - 2, 4, 4), crosshairTex);
        // Top
        GUI.DrawTexture(new Rect(cx - 1, cy - 15, 2, 10), crosshairTex);
        // Bottom
        GUI.DrawTexture(new Rect(cx - 1, cy + 5, 2, 10), crosshairTex);
        // Left
        GUI.DrawTexture(new Rect(cx - 15, cy - 1, 10, 2), crosshairTex);
        // Right
        GUI.DrawTexture(new Rect(cx + 5, cy - 1, 10, 2), crosshairTex);

        // Ammo
        if (gun != null)
        {
            string ammoText = gun.ammo + " / " + gun.reserve;
            if (gun.isReloading) ammoText += " [RELOAD]";

            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = gun.ammo <= 5 ? Color.red : Color.white;
            style.alignment = TextAnchor.LowerRight;
            GUI.Label(new Rect(Screen.width - 220, Screen.height - 50, 200, 40), ammoText, style);

            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = 16;
            nameStyle.normal.textColor = Color.gray;
            nameStyle.alignment = TextAnchor.LowerRight;
            GUI.Label(new Rect(Screen.width - 220, Screen.height - 75, 200, 25), "Глок-18", nameStyle);
        }

        // HP
        if (hp != null)
        {
            GUIStyle hpStyle = new GUIStyle();
            hpStyle.fontSize = 24;
            hpStyle.normal.textColor = hp.currentHealth > 50 ? Color.white :
                                       hp.currentHealth > 25 ? Color.yellow : Color.red;
            hpStyle.alignment = TextAnchor.LowerLeft;
            GUI.Label(new Rect(20, Screen.height - 50, 200, 40), "HP: " + hp.currentHealth, hpStyle);
        }

        // Coordinates
        GUIStyle coordStyle = new GUIStyle();
        coordStyle.fontSize = 14;
        coordStyle.normal.textColor = new Color(1, 1, 0, 0.8f);
        coordStyle.alignment = TextAnchor.UpperRight;
        Vector3 pos = transform.position;
        GUI.Label(new Rect(Screen.width - 260, 10, 250, 20),
            "X:" + pos.x.ToString("F1") + " Y:" + pos.y.ToString("F1") + " Z:" + pos.z.ToString("F1"), coordStyle);
    }
}
