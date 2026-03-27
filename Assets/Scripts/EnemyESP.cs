using UnityEngine;

public class EnemyESP : MonoBehaviour
{
    private Texture2D espTex;

    void Start()
    {
        espTex = new Texture2D(2, 2);
        espTex.SetPixel(0, 0, Color.white);
        espTex.SetPixel(1, 0, Color.white);
        espTex.SetPixel(0, 1, Color.white);
        espTex.SetPixel(1, 1, Color.white);
        espTex.Apply();
    }

    void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Find all enemies
        var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy.isDead) continue;

            Vector3 enemyPos = enemy.transform.position + Vector3.up * 1f;
            Vector3 screenPos = cam.WorldToScreenPoint(enemyPos);

            // Behind camera
            if (screenPos.z < 0) continue;

            // Convert to GUI coordinates (Y is flipped)
            float sx = screenPos.x;
            float sy = Screen.height - screenPos.y;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            // Box size based on distance
            float boxH = 800f / dist;
            float boxW = boxH * 0.4f;

            // Red box around enemy
            GUI.color = new Color(1, 0, 0, 0.8f);
            // Top
            GUI.DrawTexture(new Rect(sx - boxW / 2, sy - boxH / 2, boxW, 2), espTex);
            // Bottom
            GUI.DrawTexture(new Rect(sx - boxW / 2, sy + boxH / 2, boxW, 2), espTex);
            // Left
            GUI.DrawTexture(new Rect(sx - boxW / 2, sy - boxH / 2, 2, boxH), espTex);
            // Right
            GUI.DrawTexture(new Rect(sx + boxW / 2, sy - boxH / 2, 2, boxH), espTex);

            // Distance text
            GUIStyle style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(sx - 30, sy + boxH / 2 + 5, 60, 20), dist.ToString("F0") + "m", style);

            // Health bar
            float hpRatio = enemy.health / 100f;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(sx - boxW / 2, sy - boxH / 2 - 8, boxW, 5), espTex);
            GUI.color = hpRatio > 0.5f ? Color.green : hpRatio > 0.25f ? Color.yellow : Color.red;
            GUI.DrawTexture(new Rect(sx - boxW / 2, sy - boxH / 2 - 8, boxW * hpRatio, 5), espTex);

            GUI.color = Color.white;
        }
    }
}
