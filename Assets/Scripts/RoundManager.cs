using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    [Header("Round Settings")]
    public int maxRounds = 24;
    public int roundsToWin = 13;
    public int halfSwitch = 12;
    public float buyTime = 15f;
    public float roundTime = 115f;
    public float freezeTime = 3f;
    public float endRoundDelay = 5f;

    [Header("Spawn Points")]
    public Vector3 ctSpawnCenter = new Vector3(-25, 4.5f, -30);
    public Vector3 tSpawnCenter = new Vector3(-30, 4.5f, -30);
    public float spawnRadius = 3f;

    [Header("Economy")]
    public int startMoney = 800;
    public int maxMoney = 16000;
    public int winReward = 3250;
    public int lossReward = 1400;
    public int killReward = 300;

    [Header("State")]
    public int ctScore = 0;
    public int tScore = 0;
    public int currentRound = 0;
    public int playerMoney;
    public string playerTeam = "t"; // set by TeamSelect
    public string roundPhase = "none"; // none, freeze, buy, play, end
    public float phaseTimer = 0f;
    public bool matchOver = false;

    // Weapons for buy menu (CS2 prices)
    public static readonly Dictionary<string, int> weaponPrices = new Dictionary<string, int>()
    {
        // Pistols
        {"Glock-18", 0}, {"USP-S", 0},
        {"P250", 300}, {"Five-SeveN", 500}, {"Tec-9", 500},
        {"Desert Eagle", 700}, {"Dual Berettas", 300},
        // SMGs
        {"MP9", 1250}, {"MAC-10", 1050}, {"MP7", 1500},
        {"UMP-45", 1200}, {"P90", 2350},
        // Shotguns
        {"Nova", 1050}, {"XM1014", 2000}, {"MAG-7", 1300},
        // Rifles
        {"M4A4", 3100}, {"M4A1-S", 2900}, {"AK-47", 2700},
        {"FAMAS", 2050}, {"Galil AR", 1800},
        {"AUG", 3300}, {"SG 553", 3000},
        // Snipers
        {"AWP", 4750}, {"SSG 08", 1700},
        // Heavy
        {"M249", 5200}, {"Negev", 1700},
        // Gear
        {"Kevlar", 650}, {"Kevlar + Helmet", 1000},
        {"Zeus x27", 200}
    };

    public string currentWeaponName = "Glock-18";
    public int playerArmor = 0;
    public bool hasHelmet = false;

    private bool buyMenuOpen = false;
    private Vector2 buyMenuScroll;
    private Keyboard keyboard;
    private int kills = 0;

    // Singleton
    public static RoundManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        keyboard = Keyboard.current;
        playerMoney = startMoney;

        // Wait for TeamSelect to finish
        StartCoroutine(WaitForTeamSelect());
    }

    IEnumerator WaitForTeamSelect()
    {
        while (!TeamSelect.teamSelected)
            yield return null;

        playerTeam = TeamSelect.playerTeam;
        StartMatch();
    }

    void StartMatch()
    {
        ctScore = 0;
        tScore = 0;
        currentRound = 0;
        playerMoney = startMoney;
        matchOver = false;
        StartNewRound();
    }

    void StartNewRound()
    {
        currentRound++;

        // Half switch
        if (currentRound == halfSwitch + 1)
        {
            playerTeam = playerTeam == "ct" ? "t" : "ct";
            playerMoney = startMoney;
            currentWeaponName = playerTeam == "ct" ? "USP-S" : "Glock-18";
        }

        // Pistol round
        if (currentRound == 1 || currentRound == halfSwitch + 1)
        {
            playerMoney = 800;
            currentWeaponName = playerTeam == "ct" ? "USP-S" : "Glock-18";
            playerArmor = 0;
            hasHelmet = false;
        }

        // Spawn player
        SpawnPlayer();
        SpawnEnemies();

        // Heal player
        var ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.currentHealth = 100;
        }

        // Freeze phase
        roundPhase = "freeze";
        phaseTimer = freezeTime;

        // Disable movement and shooting during freeze/buy
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) pm.enabled = false;
        var shoot = FindFirstObjectByType<SimpleShoot>();
        if (shoot != null) shoot.enabled = false;
    }

    Vector3 FindSafeSpawn(Vector3 center, float radius)
    {
        // Try to find ground with raycast
        for (int i = 0; i < 10; i++)
        {
            Vector3 test = center + new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
            test.y = 50f;
            if (Physics.Raycast(test, Vector3.down, out RaycastHit hit, 100f))
            {
                return hit.point + Vector3.up * 1.5f;
            }
        }
        // Fallback - use center with high Y
        return center + Vector3.up * 5f;
    }

    void SpawnPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 spawnCenter = playerTeam == "ct" ? ctSpawnCenter : tSpawnCenter;
        Vector3 spawn = FindSafeSpawn(spawnCenter, spawnRadius);

        var cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.position = spawn;
        cc.enabled = true;
    }

    void SpawnEnemies()
    {
        var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            Vector3 spawnCenter = playerTeam == "ct" ? tSpawnCenter : ctSpawnCenter;
            Vector3 spawn = FindSafeSpawn(spawnCenter, spawnRadius);

            enemy.health = 100f;
            enemy.isDead = false;
            enemy.enabled = true;

            var cc = enemy.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                enemy.transform.position = spawn;
                cc.enabled = true;
            }
            else
            {
                enemy.transform.position = spawn;
            }
            enemy.transform.eulerAngles = Vector3.zero;
        }
    }

    void Update()
    {
        if (keyboard == null) keyboard = Keyboard.current;
        if (matchOver) return;

        // B key works during freeze and buy phases
        if (keyboard != null && keyboard.bKey.wasPressedThisFrame)
        {
            if (roundPhase == "freeze" || roundPhase == "buy")
            {
                buyMenuOpen = !buyMenuOpen;
                if (buyMenuOpen)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        phaseTimer -= Time.deltaTime;

        switch (roundPhase)
        {
            case "freeze":
                if (phaseTimer <= 0)
                {
                    roundPhase = "buy";
                    phaseTimer = buyTime;
                    buyMenuOpen = true;

                    // Keep movement and shooting disabled during buy
                    var shootF = FindFirstObjectByType<SimpleShoot>();
                    if (shootF != null) shootF.enabled = false;
                    var pmF = FindFirstObjectByType<PlayerMovement>();
                    if (pmF != null) pmF.enabled = false;
                }
                break;

            case "buy":
                if (phaseTimer <= 0)
                {
                    roundPhase = "play";
                    phaseTimer = roundTime;
                    buyMenuOpen = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    // Enable movement and shooting
                    var shoot2 = FindFirstObjectByType<SimpleShoot>();
                    if (shoot2 != null) shoot2.enabled = true;
                    var pm2 = FindFirstObjectByType<PlayerMovement>();
                    if (pm2 != null) pm2.enabled = true;
                }
                break;

            case "play":
                // Check round end conditions
                CheckRoundEnd();

                if (phaseTimer <= 0)
                {
                    // Time ran out - CT wins
                    EndRound("ct");
                }
                break;

            case "end":
                if (phaseTimer <= 0)
                {
                    if (ctScore >= roundsToWin || tScore >= roundsToWin || currentRound >= maxRounds)
                    {
                        matchOver = true;
                    }
                    else
                    {
                        StartNewRound();
                    }
                }
                break;
        }

        // Buy menu cursor
        if (buyMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void CheckRoundEnd()
    {
        // Check if all enemies dead
        var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        bool allEnemiesDead = true;
        foreach (var e in enemies)
        {
            if (!e.isDead) { allEnemiesDead = false; break; }
        }

        if (allEnemiesDead)
        {
            EndRound(playerTeam);
            return;
        }

        // Check if player dead
        var ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null && ph.currentHealth <= 0)
        {
            EndRound(playerTeam == "ct" ? "t" : "ct");
            return;
        }
    }

    void EndRound(string winner)
    {
        roundPhase = "end";
        phaseTimer = endRoundDelay;

        if (winner == "ct")
        {
            ctScore++;
            playerMoney += playerTeam == "ct" ? winReward : lossReward;
        }
        else
        {
            tScore++;
            playerMoney += playerTeam == "t" ? winReward : lossReward;
        }

        playerMoney = Mathf.Min(playerMoney, maxMoney);
    }

    public void OnEnemyKilled()
    {
        kills++;
        playerMoney = Mathf.Min(playerMoney + killReward, maxMoney);
    }

    void BuyWeapon(string weaponName)
    {
        if (roundPhase != "buy" && roundPhase != "freeze") return;
        if (!weaponPrices.ContainsKey(weaponName)) return;

        int price = weaponPrices[weaponName];
        if (playerMoney < price) return;

        if (weaponName == "Kevlar")
        {
            playerArmor = 100;
            playerMoney -= price;
            return;
        }
        if (weaponName == "Kevlar + Helmet")
        {
            playerArmor = 100;
            hasHelmet = true;
            playerMoney -= price;
            return;
        }
        if (weaponName == "Zeus x27")
        {
            playerMoney -= price;
            return; // Zeus is one-time use
        }

        playerMoney -= price;
        currentWeaponName = weaponName;

        // Update weapon stats
        var shoot = FindFirstObjectByType<SimpleShoot>();
        if (shoot != null)
        {
            switch (weaponName)
            {
                // Pistols
                case "Glock-18": shoot.damage = 20; shoot.maxAmmo = 20; shoot.fireRate = 0.15f; break;
                case "USP-S": shoot.damage = 25; shoot.maxAmmo = 12; shoot.fireRate = 0.17f; break;
                case "P250": shoot.damage = 28; shoot.maxAmmo = 13; shoot.fireRate = 0.15f; break;
                case "Five-SeveN": shoot.damage = 26; shoot.maxAmmo = 20; shoot.fireRate = 0.15f; break;
                case "Tec-9": shoot.damage = 24; shoot.maxAmmo = 18; shoot.fireRate = 0.12f; break;
                case "Desert Eagle": shoot.damage = 55; shoot.maxAmmo = 7; shoot.fireRate = 0.4f; break;
                case "Dual Berettas": shoot.damage = 19; shoot.maxAmmo = 30; shoot.fireRate = 0.12f; break;
                // SMGs
                case "MP9": shoot.damage = 15; shoot.maxAmmo = 30; shoot.fireRate = 0.07f; break;
                case "MAC-10": shoot.damage = 14; shoot.maxAmmo = 30; shoot.fireRate = 0.06f; break;
                case "MP7": shoot.damage = 18; shoot.maxAmmo = 30; shoot.fireRate = 0.07f; break;
                case "UMP-45": shoot.damage = 22; shoot.maxAmmo = 25; shoot.fireRate = 0.09f; break;
                case "P90": shoot.damage = 16; shoot.maxAmmo = 50; shoot.fireRate = 0.06f; break;
                // Shotguns
                case "Nova": shoot.damage = 18; shoot.maxAmmo = 8; shoot.fireRate = 0.9f; break;
                case "XM1014": shoot.damage = 15; shoot.maxAmmo = 7; shoot.fireRate = 0.35f; break;
                case "MAG-7": shoot.damage = 24; shoot.maxAmmo = 5; shoot.fireRate = 0.85f; break;
                // Rifles
                case "M4A4": shoot.damage = 30; shoot.maxAmmo = 30; shoot.fireRate = 0.08f; break;
                case "M4A1-S": shoot.damage = 32; shoot.maxAmmo = 25; shoot.fireRate = 0.09f; break;
                case "AK-47": shoot.damage = 35; shoot.maxAmmo = 30; shoot.fireRate = 0.1f; break;
                case "FAMAS": shoot.damage = 25; shoot.maxAmmo = 25; shoot.fireRate = 0.09f; break;
                case "Galil AR": shoot.damage = 24; shoot.maxAmmo = 35; shoot.fireRate = 0.09f; break;
                case "AUG": shoot.damage = 28; shoot.maxAmmo = 30; shoot.fireRate = 0.09f; break;
                case "SG 553": shoot.damage = 30; shoot.maxAmmo = 30; shoot.fireRate = 0.09f; break;
                // Snipers
                case "AWP": shoot.damage = 100; shoot.maxAmmo = 5; shoot.fireRate = 1.5f; break;
                case "SSG 08": shoot.damage = 70; shoot.maxAmmo = 10; shoot.fireRate = 0.8f; break;
                // Heavy
                case "M249": shoot.damage = 28; shoot.maxAmmo = 100; shoot.fireRate = 0.07f; break;
                case "Negev": shoot.damage = 22; shoot.maxAmmo = 150; shoot.fireRate = 0.05f; break;
            }
            shoot.ammo = shoot.maxAmmo;
            shoot.reserve = shoot.maxAmmo * 3;
        }
    }

    void OnGUI()
    {
        if (matchOver)
        {
            DrawMatchEnd();
            return;
        }

        DrawScoreboard();
        DrawPhaseInfo();

        if (buyMenuOpen && (roundPhase == "buy" || roundPhase == "freeze"))
            DrawBuyMenu();
    }

    void DrawScoreboard()
    {
        // Score at top center
        GUIStyle scoreStyle = new GUIStyle();
        scoreStyle.fontSize = 28;
        scoreStyle.alignment = TextAnchor.MiddleCenter;
        scoreStyle.fontStyle = FontStyle.Bold;

        // CT score
        scoreStyle.normal.textColor = new Color(0.3f, 0.6f, 1f);
        GUI.Label(new Rect(Screen.width / 2 - 80, 10, 60, 40), ctScore.ToString(), scoreStyle);

        // Separator
        scoreStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width / 2 - 15, 10, 30, 40), ":", scoreStyle);

        // T score
        scoreStyle.normal.textColor = new Color(1f, 0.5f, 0.2f);
        GUI.Label(new Rect(Screen.width / 2 + 20, 10, 60, 40), tScore.ToString(), scoreStyle);

        // Round info
        GUIStyle infoStyle = new GUIStyle();
        infoStyle.fontSize = 14;
        infoStyle.normal.textColor = Color.gray;
        infoStyle.alignment = TextAnchor.MiddleCenter;
        string teamLabel = playerTeam == "ct" ? "CT" : "T";
        GUI.Label(new Rect(Screen.width / 2 - 100, 45, 200, 20),
            "Round " + currentRound + "/" + maxRounds + " | " + teamLabel, infoStyle);
    }

    void DrawPhaseInfo()
    {
        GUIStyle timerStyle = new GUIStyle();
        timerStyle.alignment = TextAnchor.MiddleCenter;
        timerStyle.fontStyle = FontStyle.Bold;

        float time = Mathf.Max(0, phaseTimer);
        int min = (int)(time / 60);
        int sec = (int)(time % 60);

        switch (roundPhase)
        {
            case "freeze":
                timerStyle.fontSize = 24;
                timerStyle.normal.textColor = Color.cyan;
                GUI.Label(new Rect(Screen.width / 2 - 100, 70, 200, 35),
                    "FREEZE " + sec + "s", timerStyle);
                break;

            case "buy":
                timerStyle.fontSize = 22;
                timerStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(Screen.width / 2 - 100, 70, 200, 35),
                    "BUY TIME " + sec + "s | B - Shop", timerStyle);
                break;

            case "play":
                timerStyle.fontSize = 20;
                timerStyle.normal.textColor = time < 20 ? Color.red : Color.white;
                GUI.Label(new Rect(Screen.width / 2 - 50, 70, 100, 30),
                    min + ":" + sec.ToString("00"), timerStyle);
                break;

            case "end":
                timerStyle.fontSize = 28;
                bool playerWon = (playerTeam == "ct" && ctScore > tScore) ||
                                 (playerTeam == "t" && tScore > ctScore);
                timerStyle.normal.textColor = playerWon ? Color.green : Color.red;
                string msg = playerWon ? "ROUND WON" : "ROUND LOST";
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 30, 200, 60),
                    msg, timerStyle);
                break;
        }

        // Money
        GUIStyle moneyStyle = new GUIStyle();
        moneyStyle.fontSize = 18;
        moneyStyle.normal.textColor = new Color(0.3f, 0.9f, 0.3f);
        moneyStyle.alignment = TextAnchor.LowerLeft;
        GUI.Label(new Rect(20, Screen.height - 80, 200, 30), "$" + playerMoney, moneyStyle);
    }

    void DrawBuyMenu()
    {
        float w = 350;
        float h = 450;
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        // Background
        GUI.color = new Color(0, 0, 0, 0.9f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 22;
        titleStyle.normal.textColor = Color.yellow;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(x, y + 5, w, 35), "BUY MENU", titleStyle);

        // Money
        GUIStyle moneyStyle = new GUIStyle();
        moneyStyle.fontSize = 16;
        moneyStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);
        moneyStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x, y + 35, w, 25), "$" + playerMoney, moneyStyle);

        // Weapons list - CT and T have different weapons like CS2
        float yOff = y + 65;
        List<string> items = new List<string>();

        // Pistols
        items.Add("--- PISTOLS ---");
        if (playerTeam == "ct") {
            items.AddRange(new[] { "USP-S", "P250", "Five-SeveN", "Desert Eagle", "Dual Berettas" });
        } else {
            items.AddRange(new[] { "Glock-18", "P250", "Tec-9", "Desert Eagle", "Dual Berettas" });
        }

        // SMGs
        items.Add("--- SMG ---");
        if (playerTeam == "ct") {
            items.AddRange(new[] { "MP9", "MP7", "UMP-45", "P90" });
        } else {
            items.AddRange(new[] { "MAC-10", "MP7", "UMP-45", "P90" });
        }

        // Shotguns
        items.Add("--- SHOTGUNS ---");
        if (playerTeam == "ct") {
            items.AddRange(new[] { "Nova", "XM1014", "MAG-7" });
        } else {
            items.AddRange(new[] { "Nova", "XM1014" });
        }

        // Rifles
        items.Add("--- RIFLES ---");
        if (playerTeam == "ct") {
            items.AddRange(new[] { "M4A4", "M4A1-S", "FAMAS", "AUG" });
        } else {
            items.AddRange(new[] { "AK-47", "Galil AR", "SG 553" });
        }

        // Snipers
        items.Add("--- SNIPERS ---");
        items.AddRange(new[] { "AWP", "SSG 08" });

        // Heavy
        items.Add("--- HEAVY ---");
        items.AddRange(new[] { "M249", "Negev" });

        // Gear
        items.Add("--- GEAR ---");
        items.AddRange(new[] { "Kevlar", "Kevlar + Helmet", "Zeus x27" });

        string[] categories = items.ToArray();

        buyMenuScroll = GUI.BeginScrollView(new Rect(x + 10, yOff, w - 20, h - 80),
            buyMenuScroll, new Rect(0, 0, w - 40, categories.Length * 28));

        float itemY = 0;
        foreach (string item in categories)
        {
            if (item.StartsWith("---"))
            {
                GUIStyle catStyle = new GUIStyle();
                catStyle.fontSize = 13;
                catStyle.normal.textColor = Color.gray;
                catStyle.alignment = TextAnchor.MiddleLeft;
                catStyle.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(5, itemY, w - 40, 25), item, catStyle);
            }
            else
            {
                int price = weaponPrices.ContainsKey(item) ? weaponPrices[item] : 0;
                bool canBuy = playerMoney >= price;
                bool isEquipped = item == currentWeaponName;

                GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
                btnStyle.fontSize = 13;
                btnStyle.alignment = TextAnchor.MiddleLeft;

                if (isEquipped)
                    btnStyle.normal.textColor = Color.green;
                else if (!canBuy)
                    btnStyle.normal.textColor = Color.gray;
                else
                    btnStyle.normal.textColor = Color.white;

                string label = item + (price > 0 ? "  $" + price : "  FREE");
                if (isEquipped) label += "  [EQUIPPED]";

                if (GUI.Button(new Rect(5, itemY, w - 40, 25), label, btnStyle))
                {
                    if (canBuy && !isEquipped)
                        BuyWeapon(item);
                }
            }
            itemY += 28;
        }

        GUI.EndScrollView();
    }

    void DrawMatchEnd()
    {
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle endStyle = new GUIStyle();
        endStyle.fontSize = 40;
        endStyle.alignment = TextAnchor.MiddleCenter;
        endStyle.fontStyle = FontStyle.Bold;

        bool won = (playerTeam == "ct" && ctScore > tScore) ||
                   (playerTeam == "t" && tScore > ctScore);
        endStyle.normal.textColor = won ? Color.green : Color.red;
        string text = won ? "VICTORY!" : "DEFEAT!";
        GUI.Label(new Rect(0, Screen.height / 2 - 60, Screen.width, 60), text, endStyle);

        endStyle.fontSize = 28;
        endStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height / 2 + 10, Screen.width, 40),
            ctScore + " : " + tScore, endStyle);

        endStyle.fontSize = 16;
        endStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(0, Screen.height / 2 + 50, Screen.width, 30),
            "Kills: " + kills, endStyle);
    }
}
