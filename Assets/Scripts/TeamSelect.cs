using UnityEngine;

public class TeamSelect : MonoBehaviour
{
    public static string playerTeam = ""; // "ct" or "t"
    public static bool teamSelected = false;

    private bool showMenu = true;

    void Start()
    {
        teamSelected = false;
        playerTeam = "";
        showMenu = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player movement until team is selected
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        SimpleShoot ss = GetComponent<SimpleShoot>();
        if (ss != null) ss.enabled = false;
    }

    void OnGUI()
    {
        if (!showMenu) return;

        // Dark background
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 42;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(cx - 300, cy - 200, 600, 60), "TACTICAL EDGE", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = 20;
        subStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        subStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(cx - 300, cy - 140, 600, 40), "SELECT YOUR TEAM", subStyle);

        // CT Button (Special Forces)
        GUIStyle ctStyle = new GUIStyle(GUI.skin.button);
        ctStyle.fontSize = 24;
        ctStyle.normal.textColor = new Color(0.4f, 0.7f, 1f);
        ctStyle.hover.textColor = Color.white;
        ctStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(cx - 280, cy - 60, 250, 120), "SPECIAL FORCES\n(CT)\n\nDefend the sites", ctStyle))
        {
            SelectTeam("ct");
        }

        // T Button (Terrorists)
        GUIStyle tStyle = new GUIStyle(GUI.skin.button);
        tStyle.fontSize = 24;
        tStyle.normal.textColor = new Color(1f, 0.5f, 0.3f);
        tStyle.hover.textColor = Color.white;
        tStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(cx + 30, cy - 60, 250, 120), "TERRORISTS\n(T)\n\nPlant the bomb", tStyle))
        {
            SelectTeam("t");
        }

        // Info
        GUIStyle infoStyle = new GUIStyle();
        infoStyle.fontSize = 14;
        infoStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        infoStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(cx - 300, cy + 100, 600, 30), "WASD - Move | Mouse - Look | LMB - Shoot | R - Reload | Shift - Walk", infoStyle);
    }

    void SelectTeam(string team)
    {
        playerTeam = team;
        teamSelected = true;
        showMenu = false;

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Enable player controls
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = true;

        SimpleShoot ss = GetComponent<SimpleShoot>();
        if (ss != null) ss.enabled = true;

        // Setup enemies based on team
        SetupEnemies();

        Debug.Log("Team selected: " + team);
    }

    void SetupEnemies()
    {
        // Find all enemies and set their model
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            // If player is CT, enemies are terrorists (keep current model)
            // If player is T, enemies should look like special forces
            CharacterSetup cs = enemy.GetComponent<CharacterSetup>();
            if (cs != null)
            {
                if (playerTeam == "t")
                {
                    // Enemies are CT (special forces) - make them blue-ish
                    var renderers = enemy.GetComponentsInChildren<Renderer>();
                    foreach (var rend in renderers)
                    {
                        foreach (var mat in rend.materials)
                        {
                            // Tint towards blue/dark
                            mat.color = new Color(
                                mat.color.r * 0.6f,
                                mat.color.g * 0.6f,
                                mat.color.b * 0.8f
                            );
                        }
                    }
                }
                else
                {
                    // Enemies are T (terrorists) - make them brown/tan
                    var renderers = enemy.GetComponentsInChildren<Renderer>();
                    foreach (var rend in renderers)
                    {
                        foreach (var mat in rend.materials)
                        {
                            mat.color = new Color(
                                mat.color.r * 0.9f,
                                mat.color.g * 0.8f,
                                mat.color.b * 0.6f
                            );
                        }
                    }
                }
            }
        }
    }
}
