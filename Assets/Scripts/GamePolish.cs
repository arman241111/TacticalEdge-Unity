using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GamePolish : MonoBehaviour
{
    public static GamePolish Instance;

    // Killfeed
    private List<KillEntry> killFeed = new List<KillEntry>();
    private struct KillEntry
    {
        public string killer;
        public string victim;
        public string weapon;
        public float time;
        public bool headshot;
    }

    // Hitmarker
    private float hitmarkerTimer = 0f;
    private bool hitmarkerHeadshot = false;

    // Scoreboard
    private bool showScoreboard = false;

    // Footsteps
    private float footstepTimer = 0f;
    private CharacterController playerCC;
    private PlayerMovement playerPM;

    // FPS counter
    private float fpsTimer = 0f;
    private int fpsCount = 0;
    private int currentFPS = 0;

    // Damage indicator (red flash direction)
    private float damageFlashTimer = 0f;

    // Texture
    private Texture2D whiteTex;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        whiteTex = new Texture2D(2, 2);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.SetPixel(1, 0, Color.white);
        whiteTex.SetPixel(0, 1, Color.white);
        whiteTex.SetPixel(1, 1, Color.white);
        whiteTex.Apply();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCC = player.GetComponent<CharacterController>();
            playerPM = player.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;

        // Footsteps
        UpdateFootsteps();

        // Hitmarker fade
        if (hitmarkerTimer > 0) hitmarkerTimer -= Time.deltaTime;

        // Damage flash fade
        if (damageFlashTimer > 0) damageFlashTimer -= Time.deltaTime;

        // Scoreboard toggle
        if (keyboard != null)
            showScoreboard = keyboard.tabKey.isPressed;

        // FPS counter
        fpsTimer += Time.deltaTime;
        fpsCount++;
        if (fpsTimer >= 1f)
        {
            currentFPS = fpsCount;
            fpsCount = 0;
            fpsTimer = 0;
        }

        // Clean old killfeed entries
        killFeed.RemoveAll(k => Time.time - k.time > 5f);
    }

    void UpdateFootsteps()
    {
        if (playerCC == null || playerPM == null) return;
        if (!playerPM.enabled) return;

        Vector3 vel = new Vector3(playerCC.velocity.x, 0, playerCC.velocity.z);
        float speed = vel.magnitude;

        if (speed > 0.5f && playerCC.isGrounded)
        {
            float stepRate = playerPM.isWalking ? 0.55f : playerPM.isCrouching ? 0.7f : 0.35f;
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= stepRate)
            {
                footstepTimer = 0;
                if (SoundManager.Instance != null && !playerPM.isWalking)
                    SoundManager.Instance.PlayFootstep();
            }
        }
        else
        {
            footstepTimer = 0;
        }
    }

    // Called by other scripts
    public void ShowHitmarker(bool headshot)
    {
        hitmarkerTimer = 0.3f;
        hitmarkerHeadshot = headshot;
    }

    public void ShowDamageFlash()
    {
        damageFlashTimer = 0.5f;
    }

    public void AddKillFeed(string killer, string victim, string weapon, bool headshot)
    {
        killFeed.Insert(0, new KillEntry
        {
            killer = killer,
            victim = victim,
            weapon = weapon,
            time = Time.time,
            headshot = headshot
        });
        if (killFeed.Count > 5) killFeed.RemoveAt(5);
    }

    void OnGUI()
    {
        if (MainMenu.showMenu) return;

        DrawHitmarker();
        DrawDamageFlash();
        DrawKillfeed();
        DrawFPS();

        if (showScoreboard)
            DrawScoreboard();
    }

    void DrawHitmarker()
    {
        if (hitmarkerTimer <= 0) return;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        float alpha = hitmarkerTimer / 0.3f;
        float size = 12;

        Color col = hitmarkerHeadshot ? new Color(1, 0, 0, alpha) : new Color(1, 1, 1, alpha);
        GUI.color = col;

        // X shape hitmarker
        GUI.DrawTexture(new Rect(cx - size, cy - size, size * 0.6f, 2), whiteTex);
        GUI.DrawTexture(new Rect(cx + size * 0.4f, cy - size, size * 0.6f, 2), whiteTex);
        GUI.DrawTexture(new Rect(cx - size, cy + size, size * 0.6f, 2), whiteTex);
        GUI.DrawTexture(new Rect(cx + size * 0.4f, cy + size, size * 0.6f, 2), whiteTex);

        // Vertical parts
        GUI.DrawTexture(new Rect(cx - size, cy - size, 2, size * 0.6f), whiteTex);
        GUI.DrawTexture(new Rect(cx + size, cy - size, 2, size * 0.6f), whiteTex);
        GUI.DrawTexture(new Rect(cx - size, cy + size * 0.4f, 2, size * 0.6f), whiteTex);
        GUI.DrawTexture(new Rect(cx + size, cy + size * 0.4f, 2, size * 0.6f), whiteTex);

        if (hitmarkerHeadshot)
        {
            GUIStyle hsStyle = new GUIStyle();
            hsStyle.fontSize = 16;
            hsStyle.normal.textColor = new Color(1, 0, 0, alpha);
            hsStyle.alignment = TextAnchor.MiddleCenter;
            hsStyle.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(cx - 50, cy - 35, 100, 20), "HEADSHOT", hsStyle);
        }

        GUI.color = Color.white;
    }

    void DrawDamageFlash()
    {
        if (damageFlashTimer <= 0) return;
        float alpha = damageFlashTimer / 0.5f * 0.25f;
        GUI.color = new Color(1, 0, 0, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTex);
        GUI.color = Color.white;
    }

    void DrawKillfeed()
    {
        float x = Screen.width - 320;
        float y = 100;

        for (int i = 0; i < killFeed.Count; i++)
        {
            var entry = killFeed[i];
            float age = Time.time - entry.time;
            float alpha = Mathf.Clamp01(1 - (age - 3f) / 2f);

            // Background
            GUI.color = new Color(0, 0, 0, 0.5f * alpha);
            GUI.DrawTexture(new Rect(x, y + i * 28, 310, 25), whiteTex);
            GUI.color = Color.white;

            GUIStyle feedStyle = new GUIStyle();
            feedStyle.fontSize = 13;
            feedStyle.alignment = TextAnchor.MiddleLeft;

            // Killer name
            feedStyle.normal.textColor = new Color(0.3f, 0.6f, 1f, alpha);
            GUI.Label(new Rect(x + 5, y + i * 28, 100, 25), entry.killer, feedStyle);

            // Weapon icon (text)
            feedStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, alpha);
            feedStyle.alignment = TextAnchor.MiddleCenter;
            string weaponIcon = entry.headshot ? "[HS] " + entry.weapon : entry.weapon;
            GUI.Label(new Rect(x + 100, y + i * 28, 110, 25), weaponIcon, feedStyle);

            // Victim name
            feedStyle.normal.textColor = new Color(1f, 0.4f, 0.2f, alpha);
            feedStyle.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(x + 210, y + i * 28, 95, 25), entry.victim, feedStyle);
        }
    }

    void DrawScoreboard()
    {
        float w = 500;
        float h = 350;
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        // Background
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(x, y, w, h), whiteTex);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 22;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(x, y + 10, w, 35), "SCOREBOARD", titleStyle);

        // Headers
        GUIStyle headerStyle = new GUIStyle();
        headerStyle.fontSize = 13;
        headerStyle.normal.textColor = Color.gray;
        headerStyle.fontStyle = FontStyle.Bold;

        float row = y + 55;
        GUI.Label(new Rect(x + 15, row, 150, 20), "PLAYER", headerStyle);
        headerStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x + 200, row, 60, 20), "K", headerStyle);
        GUI.Label(new Rect(x + 260, row, 60, 20), "D", headerStyle);
        GUI.Label(new Rect(x + 320, row, 60, 20), "SCORE", headerStyle);
        GUI.Label(new Rect(x + 400, row, 60, 20), "PING", headerStyle);

        // Separator
        GUI.color = new Color(0.3f, 0.3f, 0.3f);
        GUI.DrawTexture(new Rect(x + 10, row + 25, w - 20, 1), whiteTex);
        GUI.color = Color.white;

        // Player row
        row += 35;
        var rm = RoundManager.Instance;
        int kills = rm != null ? rm.ctScore + rm.tScore : 0;

        GUIStyle playerStyle = new GUIStyle();
        playerStyle.fontSize = 14;
        playerStyle.normal.textColor = new Color(0.3f, 0.7f, 1f);
        GUI.Label(new Rect(x + 15, row, 150, 22), "You (" + TeamSelect.playerTeam.ToUpper() + ")", playerStyle);

        playerStyle.alignment = TextAnchor.MiddleCenter;
        playerStyle.normal.textColor = Color.white;
        var mm = MainMenu.Instance;
        string k = mm != null ? mm.ToString() : "0";
        GUI.Label(new Rect(x + 200, row, 60, 22), "—", playerStyle);
        GUI.Label(new Rect(x + 260, row, 60, 22), "—", playerStyle);
        GUI.Label(new Rect(x + 320, row, 60, 22), kills.ToString(), playerStyle);
        GUI.Label(new Rect(x + 400, row, 60, 22), "0ms", playerStyle);

        // Enemy rows
        var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        for (int i = 0; i < Mathf.Min(enemies.Length, 10); i++)
        {
            row += 25;
            GUIStyle enemyStyle = new GUIStyle();
            enemyStyle.fontSize = 13;
            enemyStyle.normal.textColor = enemies[i].isDead ? Color.gray : new Color(1f, 0.5f, 0.2f);

            string status = enemies[i].isDead ? " [DEAD]" : "";
            GUI.Label(new Rect(x + 15, row, 180, 22), "Bot " + (i + 1) + status, enemyStyle);

            enemyStyle.alignment = TextAnchor.MiddleCenter;
            enemyStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(x + 200, row, 60, 22), "—", enemyStyle);
            GUI.Label(new Rect(x + 260, row, 60, 22), "—", enemyStyle);
            GUI.Label(new Rect(x + 320, row, 60, 22), "0", enemyStyle);
            GUI.Label(new Rect(x + 400, row, 60, 22), "BOT", enemyStyle);
        }
    }

    void DrawFPS()
    {
        GUIStyle fpsStyle = new GUIStyle();
        fpsStyle.fontSize = 12;
        fpsStyle.normal.textColor = currentFPS >= 60 ? Color.green : currentFPS >= 30 ? Color.yellow : Color.red;
        fpsStyle.alignment = TextAnchor.UpperLeft;
        GUI.Label(new Rect(10, 10, 80, 20), "FPS: " + currentFPS, fpsStyle);
    }
}
