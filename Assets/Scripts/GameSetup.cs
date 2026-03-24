using UnityEngine;
using UnityEngine.UI;

public class GameSetup : MonoBehaviour
{
    void Start()
    {
        // Setup weapon system on player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            WeaponSystem ws = player.GetComponent<WeaponSystem>();
            if (ws == null)
                ws = player.AddComponent<WeaponSystem>();
            ws.playerCamera = Camera.main;
            ws.weaponName = "Glock-18";
            ws.damage = 25f;
            ws.fireRate = 0.15f;
            ws.isAutomatic = false;
            ws.maxAmmo = 20;
            ws.currentAmmo = 20;
            ws.reserveAmmo = 60;
            ws.range = 80f;
            ws.spread = 0.02f;
        }

        // Create crosshair UI
        CreateCrosshair();

        // Create HUD
        CreateHUD();
    }

    void CreateCrosshair()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Center dot
        CreateCrosshairLine(canvasObj.transform, 0, 0, 4, 4, "Dot");

        // Top line
        CreateCrosshairLine(canvasObj.transform, 0, 10, 2, 12, "Top");

        // Bottom line
        CreateCrosshairLine(canvasObj.transform, 0, -10, 2, 12, "Bottom");

        // Left line
        CreateCrosshairLine(canvasObj.transform, -10, 0, 12, 2, "Left");

        // Right line
        CreateCrosshairLine(canvasObj.transform, 10, 0, 12, 2, "Right");
    }

    void CreateCrosshairLine(Transform parent, float x, float y, float w, float h, string name)
    {
        GameObject line = new GameObject("Crosshair_" + name);
        line.transform.SetParent(parent);
        RectTransform rt = line.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        Image img = line.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.9f);
    }

    void CreateHUD()
    {
        // Find or create canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // HP text - bottom left
        CreateHUDText(canvas.transform, "HPText", "HP: 100", TextAnchor.LowerLeft,
            new Vector2(20, 20), new Vector2(200, 40));

        // Ammo text - bottom right
        CreateHUDText(canvas.transform, "AmmoText", "20 / 60", TextAnchor.LowerRight,
            new Vector2(-20, 20), new Vector2(200, 40));

        // Weapon name - above ammo
        CreateHUDText(canvas.transform, "WeaponText", "Glock-18", TextAnchor.LowerRight,
            new Vector2(-20, 55), new Vector2(200, 30));

        // Kill feed area - top right
        CreateHUDText(canvas.transform, "KillFeed", "", TextAnchor.UpperRight,
            new Vector2(-20, -20), new Vector2(300, 100));
    }

    void CreateHUDText(Transform parent, string name, string text, TextAnchor anchor,
        Vector2 offset, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        RectTransform rt = obj.AddComponent<RectTransform>();

        // Set anchors based on alignment
        switch (anchor)
        {
            case TextAnchor.LowerLeft:
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                break;
            case TextAnchor.LowerRight:
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                break;
            case TextAnchor.UpperRight:
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                break;
        }

        rt.anchoredPosition = offset;
        rt.sizeDelta = size;

        Text txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 22;
        txt.color = Color.white;
        txt.alignment = anchor;

        // Shadow for readability
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(1, -1);
    }

    void Update()
    {
        // Update HUD every frame
        UpdateHUD();
    }

    void UpdateHUD()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        WeaponSystem ws = player.GetComponent<WeaponSystem>();

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // HP
        Text hpText = FindHUDText(canvas.transform, "HPText");
        if (hpText != null && ph != null)
        {
            hpText.text = "HP: " + ph.currentHealth;
            hpText.color = ph.currentHealth > 50 ? Color.white :
                           ph.currentHealth > 25 ? Color.yellow : Color.red;
        }

        // Ammo
        Text ammoText = FindHUDText(canvas.transform, "AmmoText");
        if (ammoText != null && ws != null)
        {
            string reload = ws.isReloading ? " [R]" : "";
            ammoText.text = ws.currentAmmo + " / " + ws.reserveAmmo + reload;
            ammoText.color = ws.currentAmmo <= 5 ? Color.red : Color.white;
        }

        // Weapon name
        Text weaponText = FindHUDText(canvas.transform, "WeaponText");
        if (weaponText != null && ws != null)
            weaponText.text = ws.weaponName;
    }

    Text FindHUDText(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null) return t.GetComponent<Text>();
        return null;
    }
}
