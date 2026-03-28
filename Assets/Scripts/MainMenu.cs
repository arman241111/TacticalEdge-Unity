using UnityEngine;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    public static bool showMenu = true;

    // Tabs
    private string currentTab = "ИГРАТЬ";
    private string[] tabs = { "ИГРАТЬ", "ПРОФИЛЬ", "ИНВЕНТАРЬ", "МАГАЗИН", "НАСТРОЙКИ" };

    // Player data
    private string playerName = "Player";
    private int playerLevel = 1;
    private int playerXP = 0;
    private int playerCoins = 1000;
    private int playerKeys = 3;
    private int totalKills = 0;
    private int totalDeaths = 0;
    private int totalWins = 0;
    private int gamesPlayed = 0;

    // Inventory
    private List<string> ownedSkins = new List<string>() {
        "Glock-18 | Default",
        "USP-S | Default",
        "AK-47 | Default",
        "M4A4 | Default",
        "AWP | Default"
    };
    private string equippedSkin = "";

    // Shop items
    private Dictionary<string, int> shopItems = new Dictionary<string, int>() {
        {"AK-47 | Neon Strike", 2500},
        {"M4A4 | Cyber Ghost", 3000},
        {"AWP | Thunder", 5000},
        {"USP-S | Dark Water", 1500},
        {"Glock-18 | Fade", 2000},
        {"Desert Eagle | Gold", 4000},
        {"AK-47 | Fire Storm", 3500},
        {"M4A4 | Shadow Ops", 2800},
        {"AWP | Ice Dragon", 6000},
        {"Key", 500}
    };

    // Settings
    private float sensitivity = 2f;
    private float volume = 1f;
    private int fov = 75;
    private bool showFPS = false;

    // Scroll
    private Vector2 scrollPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        LoadData();
    }

    void OnGUI()
    {
        if (!showMenu) return;

        float W = Screen.width;
        float H = Screen.height;

        // Full screen dark background
        GUI.color = new Color(0.08f, 0.08f, 0.12f, 1f);
        GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Top bar
        DrawTopBar(W);

        // Tab bar
        DrawTabBar(W);

        // Content
        float contentY = 100;
        float contentH = H - 110;

        switch (currentTab)
        {
            case "ИГРАТЬ": DrawPlayTab(W, contentY, contentH); break;
            case "ПРОФИЛЬ": DrawProfileTab(W, contentY, contentH); break;
            case "ИНВЕНТАРЬ": DrawInventoryTab(W, contentY, contentH); break;
            case "МАГАЗИН": DrawShopTab(W, contentY, contentH); break;
            case "НАСТРОЙКИ": DrawSettingsTab(W, contentY, contentH); break;
        }
    }

    void DrawTopBar(float W)
    {
        // Logo
        GUIStyle logoStyle = new GUIStyle();
        logoStyle.fontSize = 28;
        logoStyle.normal.textColor = new Color(0.9f, 0.75f, 0.3f);
        logoStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(20, 8, 300, 40), "TACTICAL EDGE", logoStyle);

        // Player info
        GUIStyle infoStyle = new GUIStyle();
        infoStyle.fontSize = 14;
        infoStyle.normal.textColor = Color.white;
        infoStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(W - 350, 5, 150, 20), playerName, infoStyle);

        infoStyle.fontSize = 12;
        infoStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(W - 350, 22, 150, 20), "Level " + playerLevel, infoStyle);

        // Currency
        GUIStyle coinStyle = new GUIStyle();
        coinStyle.fontSize = 14;
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        coinStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(W - 180, 5, 80, 20), playerCoins.ToString(), coinStyle);

        GUIStyle keyStyle = new GUIStyle();
        keyStyle.fontSize = 14;
        keyStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);
        keyStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(W - 90, 5, 80, 20), playerKeys + " Кл.", keyStyle);
    }

    void DrawTabBar(float W)
    {
        float tabW = W / tabs.Length;
        float tabY = 50;

        for (int i = 0; i < tabs.Length; i++)
        {
            bool selected = tabs[i] == currentTab;

            // Tab background
            if (selected)
            {
                GUI.color = new Color(0.9f, 0.75f, 0.3f, 0.15f);
                GUI.DrawTexture(new Rect(i * tabW, tabY, tabW, 40), Texture2D.whiteTexture);
                // Bottom line
                GUI.color = new Color(0.9f, 0.75f, 0.3f, 1f);
                GUI.DrawTexture(new Rect(i * tabW, tabY + 37, tabW, 3), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            GUIStyle tabStyle = new GUIStyle();
            tabStyle.fontSize = 16;
            tabStyle.normal.textColor = selected ? new Color(0.9f, 0.75f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);
            tabStyle.alignment = TextAnchor.MiddleCenter;
            tabStyle.fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;

            if (GUI.Button(new Rect(i * tabW, tabY, tabW, 40), "", GUIStyle.none))
                currentTab = tabs[i];

            GUI.Label(new Rect(i * tabW, tabY, tabW, 40), tabs[i], tabStyle);
        }

        // Separator line
        GUI.color = new Color(0.3f, 0.3f, 0.3f);
        GUI.DrawTexture(new Rect(0, tabY + 40, W, 1), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawPlayTab(float W, float Y, float H)
    {
        float cx = W / 2;

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 36;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(0, Y + 40, W, 50), "ВЫБЕРИТЕ РЕЖИМ", titleStyle);

        // Game mode buttons
        string[] modes = { "СОРЕВНОВАТЕЛЬНЫЙ", "ПЕРЕСТРЕЛКА", "ГОНКА ВООРУЖЕНИЙ", "ОБЫЧНЫЙ" };
        string[] descs = {
            "5v5 | Раунды | Экономика | Бомба",
            "Все против всех | Респавн | Без раундов",
            "Убийство = Новое оружие | Первый до конца",
            "Расслабленный | Больше денег | Респавн"
        };
        Color[] colors = {
            new Color(0.9f, 0.75f, 0.3f),
            new Color(0.3f, 0.8f, 0.4f),
            new Color(0.8f, 0.3f, 0.8f),
            new Color(0.3f, 0.6f, 0.9f)
        };

        float btnW = 300;
        float btnH = 80;
        float startX = cx - btnW - 20;
        float startY = Y + 120;

        for (int i = 0; i < modes.Length; i++)
        {
            float bx = startX + (i % 2) * (btnW + 40);
            float by = startY + (i / 2) * (btnH + 20);

            // Button bg
            GUI.color = new Color(0.15f, 0.15f, 0.2f);
            GUI.DrawTexture(new Rect(bx, by, btnW, btnH), Texture2D.whiteTexture);
            // Left accent
            GUI.color = colors[i];
            GUI.DrawTexture(new Rect(bx, by, 4, btnH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(bx, by, btnW, btnH), "", GUIStyle.none))
            {
                StartGame(modes[i]);
            }

            GUIStyle modeStyle = new GUIStyle();
            modeStyle.fontSize = 20;
            modeStyle.normal.textColor = colors[i];
            modeStyle.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(bx + 15, by + 12, btnW - 20, 30), modes[i], modeStyle);

            GUIStyle descStyle = new GUIStyle();
            descStyle.fontSize = 12;
            descStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(bx + 15, by + 42, btnW - 20, 25), descs[i], descStyle);
        }

        // Solo button
        if (GUI.Button(new Rect(cx - 100, startY + 200, 200, 40), ""))
        {
            StartGame("SOLO");
        }
        GUI.color = new Color(0.2f, 0.2f, 0.25f);
        GUI.DrawTexture(new Rect(cx - 100, startY + 200, 200, 40), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUIStyle soloStyle = new GUIStyle();
        soloStyle.fontSize = 16;
        soloStyle.normal.textColor = Color.gray;
        soloStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(cx - 100, startY + 200, 200, 40), "ИГРАТЬ С БОТАМИ", soloStyle);
    }

    void DrawProfileTab(float W, float Y, float H)
    {
        float cx = W / 2;

        // Avatar area
        GUI.color = new Color(0.15f, 0.15f, 0.2f);
        GUI.DrawTexture(new Rect(cx - 200, Y + 20, 400, 150), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 28;
        nameStyle.normal.textColor = Color.white;
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(cx - 200, Y + 30, 400, 40), playerName, nameStyle);

        GUIStyle lvlStyle = new GUIStyle();
        lvlStyle.fontSize = 18;
        lvlStyle.normal.textColor = new Color(0.9f, 0.75f, 0.3f);
        lvlStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(cx - 200, Y + 70, 400, 30), "LEVEL " + playerLevel, lvlStyle);

        // XP bar
        GUI.color = new Color(0.2f, 0.2f, 0.25f);
        GUI.DrawTexture(new Rect(cx - 150, Y + 110, 300, 15), Texture2D.whiteTexture);
        GUI.color = new Color(0.9f, 0.75f, 0.3f);
        float xpRatio = (float)playerXP / (playerLevel * 1000);
        GUI.DrawTexture(new Rect(cx - 150, Y + 110, 300 * xpRatio, 15), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle xpStyle = new GUIStyle();
        xpStyle.fontSize = 11;
        xpStyle.normal.textColor = Color.white;
        xpStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(cx - 150, Y + 108, 300, 18), playerXP + " / " + (playerLevel * 1000) + " XP", xpStyle);

        // Stats
        float statY = Y + 190;
        string[] statNames = { "Убийств", "Смертей", "У/С", "Побед", "Игр сыграно" };
        string[] statValues = {
            totalKills.ToString(),
            totalDeaths.ToString(),
            totalDeaths > 0 ? ((float)totalKills / totalDeaths).ToString("F2") : "0",
            totalWins.ToString(),
            gamesPlayed.ToString()
        };

        GUI.color = new Color(0.12f, 0.12f, 0.16f);
        GUI.DrawTexture(new Rect(cx - 200, statY, 400, statNames.Length * 35 + 20), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle statLabelStyle = new GUIStyle();
        statLabelStyle.fontSize = 15;
        statLabelStyle.normal.textColor = Color.gray;

        GUIStyle statValueStyle = new GUIStyle();
        statValueStyle.fontSize = 15;
        statValueStyle.normal.textColor = Color.white;
        statValueStyle.alignment = TextAnchor.MiddleRight;

        for (int i = 0; i < statNames.Length; i++)
        {
            GUI.Label(new Rect(cx - 180, statY + 10 + i * 35, 200, 30), statNames[i], statLabelStyle);
            GUI.Label(new Rect(cx - 10, statY + 10 + i * 35, 190, 30), statValues[i], statValueStyle);
        }
    }

    void DrawInventoryTab(float W, float Y, float H)
    {
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 20;
        titleStyle.normal.textColor = Color.white;
        titleStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(30, Y + 10, 300, 30), "МОИ СКИНЫ (" + ownedSkins.Count + ")", titleStyle);

        scrollPos = GUI.BeginScrollView(new Rect(20, Y + 50, W - 40, H - 60), scrollPos,
            new Rect(0, 0, W - 60, ownedSkins.Count * 55 + 20));

        for (int i = 0; i < ownedSkins.Count; i++)
        {
            float iy = i * 55;
            bool equipped = ownedSkins[i] == equippedSkin;

            GUI.color = equipped ? new Color(0.15f, 0.2f, 0.15f) : new Color(0.12f, 0.12f, 0.16f);
            GUI.DrawTexture(new Rect(0, iy, W - 60, 48), Texture2D.whiteTexture);
            if (equipped)
            {
                GUI.color = new Color(0.3f, 0.8f, 0.3f);
                GUI.DrawTexture(new Rect(0, iy, 3, 48), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            GUIStyle skinStyle = new GUIStyle();
            skinStyle.fontSize = 16;
            skinStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(15, iy + 12, 300, 25), ownedSkins[i], skinStyle);

            if (!equipped)
            {
                if (GUI.Button(new Rect(W - 180, iy + 8, 100, 32), "НАДЕТЬ"))
                {
                    equippedSkin = ownedSkins[i];
                }
            }
            else
            {
                GUIStyle eqStyle = new GUIStyle();
                eqStyle.fontSize = 13;
                eqStyle.normal.textColor = new Color(0.3f, 0.8f, 0.3f);
                eqStyle.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(W - 200, iy + 12, 120, 25), "НАДЕТО", eqStyle);
            }
        }

        GUI.EndScrollView();
    }

    void DrawShopTab(float W, float Y, float H)
    {
        // Currency
        GUIStyle coinStyle = new GUIStyle();
        coinStyle.fontSize = 18;
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(30, Y + 10, 200, 30), "Монеты: " + playerCoins, coinStyle);

        scrollPos = GUI.BeginScrollView(new Rect(20, Y + 50, W - 40, H - 60), scrollPos,
            new Rect(0, 0, W - 60, shopItems.Count * 60 + 20));

        int idx = 0;
        foreach (var item in shopItems)
        {
            float iy = idx * 60;
            bool owned = ownedSkins.Contains(item.Key);
            bool canBuy = playerCoins >= item.Value && !owned;

            GUI.color = new Color(0.12f, 0.12f, 0.16f);
            GUI.DrawTexture(new Rect(0, iy, W - 60, 52), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle itemStyle = new GUIStyle();
            itemStyle.fontSize = 16;
            itemStyle.normal.textColor = owned ? Color.gray : Color.white;
            GUI.Label(new Rect(15, iy + 14, 300, 25), item.Key, itemStyle);

            GUIStyle priceStyle = new GUIStyle();
            priceStyle.fontSize = 14;
            priceStyle.normal.textColor = canBuy ? new Color(1f, 0.85f, 0.2f) : Color.gray;
            priceStyle.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(W - 300, iy + 14, 100, 25), item.Value.ToString(), priceStyle);

            if (owned)
            {
                GUIStyle ownStyle = new GUIStyle();
                ownStyle.fontSize = 13;
                ownStyle.normal.textColor = new Color(0.3f, 0.8f, 0.3f);
                ownStyle.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(W - 180, iy + 14, 100, 25), "КУПЛЕНО", ownStyle);
            }
            else if (GUI.Button(new Rect(W - 180, iy + 10, 100, 32), canBuy ? "КУПИТЬ" : "---"))
            {
                if (canBuy) BuyItem(item.Key, item.Value);
            }

            idx++;
        }

        GUI.EndScrollView();
    }

    void DrawSettingsTab(float W, float Y, float H)
    {
        float cx = W / 2;
        float settW = 400;
        float sx = cx - settW / 2;
        float sy = Y + 30;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 16;
        labelStyle.normal.textColor = Color.white;

        GUIStyle valStyle = new GUIStyle();
        valStyle.fontSize = 14;
        valStyle.normal.textColor = new Color(0.9f, 0.75f, 0.3f);
        valStyle.alignment = TextAnchor.MiddleRight;

        // Sensitivity
        GUI.Label(new Rect(sx, sy, 200, 25), "Чувствительность", labelStyle);
        GUI.Label(new Rect(sx + settW - 50, sy, 50, 25), sensitivity.ToString("F1"), valStyle);
        sensitivity = GUI.HorizontalSlider(new Rect(sx, sy + 30, settW, 20), sensitivity, 0.5f, 10f);
        sy += 65;

        // Volume
        GUI.Label(new Rect(sx, sy, 200, 25), "Громкость", labelStyle);
        GUI.Label(new Rect(sx + settW - 50, sy, 50, 25), (volume * 100).ToString("F0") + "%", valStyle);
        volume = GUI.HorizontalSlider(new Rect(sx, sy + 30, settW, 20), volume, 0f, 1f);
        sy += 65;

        // FOV
        GUI.Label(new Rect(sx, sy, 200, 25), "Поле зрения", labelStyle);
        GUI.Label(new Rect(sx + settW - 50, sy, 50, 25), fov.ToString(), valStyle);
        fov = (int)GUI.HorizontalSlider(new Rect(sx, sy + 30, settW, 20), fov, 60, 120);
        sy += 65;

        // Show FPS
        GUI.Label(new Rect(sx, sy, 200, 25), "Показать FPS", labelStyle);
        showFPS = GUI.Toggle(new Rect(sx + settW - 30, sy, 30, 25), showFPS, "");
        sy += 45;

        // Player name
        GUI.Label(new Rect(sx, sy, 200, 25), "Имя игрока", labelStyle);
        playerName = GUI.TextField(new Rect(sx + 150, sy, settW - 150, 25), playerName, 20);
        sy += 45;

        // Apply button
        if (GUI.Button(new Rect(cx - 80, sy + 20, 160, 40), "ПРИМЕНИТЬ"))
        {
            ApplySettings();
            SaveData();
        }
    }

    void StartGame(string mode)
    {
        showMenu = false;
        SaveData();

        // Enable NetworkManager to show lobby
        var nm = FindFirstObjectByType<NetworkManager>();
        if (nm != null)
        {
            nm.enabled = true;
        }
    }

    void BuyItem(string name, int price)
    {
        if (playerCoins >= price)
        {
            playerCoins -= price;
            if (name == "Key")
                playerKeys++;
            else
                ownedSkins.Add(name);
            SaveData();
        }
    }

    void ApplySettings()
    {
        // Apply sensitivity
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) pm.mouseSensitivity = sensitivity;

        // Apply FOV
        var cam = Camera.main;
        if (cam != null) cam.fieldOfView = fov;

        // Apply volume
        AudioListener.volume = volume;
    }

    void SaveData()
    {
        PlayerPrefs.SetString("playerName", playerName);
        PlayerPrefs.SetInt("playerLevel", playerLevel);
        PlayerPrefs.SetInt("playerXP", playerXP);
        PlayerPrefs.SetInt("playerCoins", playerCoins);
        PlayerPrefs.SetInt("playerKeys", playerKeys);
        PlayerPrefs.SetInt("totalKills", totalKills);
        PlayerPrefs.SetInt("totalDeaths", totalDeaths);
        PlayerPrefs.SetInt("totalWins", totalWins);
        PlayerPrefs.SetInt("gamesPlayed", gamesPlayed);
        PlayerPrefs.SetFloat("sensitivity", sensitivity);
        PlayerPrefs.SetFloat("volume", volume);
        PlayerPrefs.SetInt("fov", fov);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        playerName = PlayerPrefs.GetString("playerName", "Player");
        playerLevel = PlayerPrefs.GetInt("playerLevel", 1);
        playerXP = PlayerPrefs.GetInt("playerXP", 0);
        playerCoins = PlayerPrefs.GetInt("playerCoins", 1000);
        playerKeys = PlayerPrefs.GetInt("playerKeys", 3);
        totalKills = PlayerPrefs.GetInt("totalKills", 0);
        totalDeaths = PlayerPrefs.GetInt("totalDeaths", 0);
        totalWins = PlayerPrefs.GetInt("totalWins", 0);
        gamesPlayed = PlayerPrefs.GetInt("gamesPlayed", 0);
        sensitivity = PlayerPrefs.GetFloat("sensitivity", 2f);
        volume = PlayerPrefs.GetFloat("volume", 1f);
        fov = PlayerPrefs.GetInt("fov", 75);
    }

    // Called by other scripts when game ends
    public void AddKills(int k) { totalKills += k; SaveData(); }
    public void AddDeaths(int d) { totalDeaths += d; SaveData(); }
    public void AddWin() { totalWins++; gamesPlayed++; playerXP += 100; CheckLevelUp(); SaveData(); }
    public void AddLoss() { gamesPlayed++; playerXP += 30; CheckLevelUp(); SaveData(); }
    public void AddCoins(int c) { playerCoins += c; SaveData(); }

    void CheckLevelUp()
    {
        while (playerXP >= playerLevel * 1000)
        {
            playerXP -= playerLevel * 1000;
            playerLevel++;
            playerCoins += 500; // bonus for level up
        }
    }
}
