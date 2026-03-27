using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [Header("State")]
    public bool isConnected = false;
    public bool isInRoom = false;
    public string roomName = "";
    public int playersInRoom = 0;

    private bool showLobbyUI = true;
    private string inputRoomName = "";
    private string statusText = "";
    private Vector2 scrollPos;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Free cursor for lobby
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player controls until in room
        DisablePlayerControls();

        // Connect to Photon
        PhotonNetwork.ConnectUsingSettings();
        statusText = "Connecting to server...";
    }

    void DisablePlayerControls()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
            var ss = player.GetComponent<SimpleShoot>();
            if (ss != null) ss.enabled = false;
        }
    }

    void EnablePlayerControls()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = true;
            var ss = player.GetComponent<SimpleShoot>();
            if (ss != null) ss.enabled = true;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnConnectedToMaster()
    {
        isConnected = true;
        statusText = "Connected! Create or join a room.";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText = "In lobby. Create or join a room.";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnected = false;
        statusText = "Disconnected: " + cause;
    }

    public void CreateRoom(string name)
    {
        if (!isConnected) return;
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 10;
        options.IsVisible = true;
        PhotonNetwork.CreateRoom(name, options);
        statusText = "Creating room: " + name + "...";
    }

    public void JoinRoom(string name)
    {
        if (!isConnected) return;
        PhotonNetwork.JoinRoom(name);
        statusText = "Joining room: " + name + "...";
    }

    private bool showTeamSelect = false;

    public override void OnJoinedRoom()
    {
        isInRoom = true;
        roomName = PhotonNetwork.CurrentRoom.Name;
        playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        statusText = "In room: " + roomName + " (" + playersInRoom + " players)";
        showLobbyUI = false;
        showTeamSelect = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText = "Failed to join: " + message;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText = "Failed to create: " + message;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        statusText = newPlayer.NickName + " joined! (" + playersInRoom + " players)";
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        statusText = otherPlayer.NickName + " left. (" + playersInRoom + " players)";
    }

    void OnGUI()
    {
        if (showTeamSelect)
        {
            DrawTeamSelect();
            return;
        }
        if (!showLobbyUI) return;

        float w = 400;
        float h = 350;
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        // Background
        GUI.color = new Color(0, 0, 0, 0.92f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 28;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(x, y + 10, w, 40), "TACTICAL EDGE", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = 14;
        subStyle.normal.textColor = Color.gray;
        subStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x, y + 45, w, 25), "MULTIPLAYER", subStyle);

        // Status
        GUIStyle statusStyle = new GUIStyle();
        statusStyle.fontSize = 13;
        statusStyle.normal.textColor = isConnected ? Color.green : Color.yellow;
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.wordWrap = true;
        GUI.Label(new Rect(x + 10, y + 75, w - 20, 30), statusText, statusStyle);

        if (!isConnected)
            return;

        // Room name input
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 15;
        labelStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x + 20, y + 115, 120, 30), "Room name:", labelStyle);
        inputRoomName = GUI.TextField(new Rect(x + 140, y + 115, w - 180, 28), inputRoomName, 20);

        // Create button
        if (GUI.Button(new Rect(x + 20, y + 155, (w - 60) / 2, 40), "CREATE ROOM"))
        {
            if (inputRoomName.Length > 0)
                CreateRoom(inputRoomName);
        }

        // Join button
        if (GUI.Button(new Rect(x + 30 + (w - 60) / 2, y + 155, (w - 60) / 2, 40), "JOIN ROOM"))
        {
            if (inputRoomName.Length > 0)
                JoinRoom(inputRoomName);
        }

        // Quick match
        if (GUI.Button(new Rect(x + 20, y + 205, w - 40, 35), "QUICK MATCH"))
        {
            PhotonNetwork.JoinRandomRoom();
        }

        // Solo play
        if (GUI.Button(new Rect(x + 20, y + 250, w - 40, 35), "SOLO (OFFLINE)"))
        {
            showLobbyUI = false;
            PhotonNetwork.OfflineMode = true;
            showTeamSelect = true;
        }

        // Player count
        GUIStyle infoStyle = new GUIStyle();
        infoStyle.fontSize = 12;
        infoStyle.normal.textColor = Color.gray;
        infoStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x, y + h - 30, w, 25),
            "Players online: " + PhotonNetwork.CountOfPlayers, infoStyle);
    }

    void DrawTeamSelect()
    {
        float w = 500;
        float h = 250;
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        GUI.color = new Color(0, 0, 0, 0.9f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 32;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(x, y + 15, w, 45), "SELECT TEAM", titleStyle);

        GUIStyle infoStyle = new GUIStyle();
        infoStyle.fontSize = 14;
        infoStyle.normal.textColor = Color.gray;
        infoStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x, y + 55, w, 25), "Room: " + roomName + " | Players: " + playersInRoom, infoStyle);

        // CT Button
        if (GUI.Button(new Rect(x + 20, y + 95, w / 2 - 30, 100), ""))
        {
            SelectTeam("ct");
        }
        GUIStyle ctStyle = new GUIStyle();
        ctStyle.fontSize = 24;
        ctStyle.normal.textColor = new Color(0.3f, 0.6f, 1f);
        ctStyle.alignment = TextAnchor.MiddleCenter;
        ctStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(x + 20, y + 95, w / 2 - 30, 50), "SPECIAL FORCES", ctStyle);
        ctStyle.fontSize = 14;
        ctStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(x + 20, y + 145, w / 2 - 30, 40), "(CT)", ctStyle);

        // T Button
        if (GUI.Button(new Rect(x + w / 2 + 10, y + 95, w / 2 - 30, 100), ""))
        {
            SelectTeam("t");
        }
        GUIStyle tStyle = new GUIStyle();
        tStyle.fontSize = 24;
        tStyle.normal.textColor = new Color(1f, 0.5f, 0.2f);
        tStyle.alignment = TextAnchor.MiddleCenter;
        tStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(x + w / 2 + 10, y + 95, w / 2 - 30, 50), "TERRORISTS", tStyle);
        tStyle.fontSize = 14;
        tStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(x + w / 2 + 10, y + 145, w / 2 - 30, 40), "(T)", tStyle);
    }

    void SelectTeam(string team)
    {
        showTeamSelect = false;
        TeamSelect.teamSelected = true;
        TeamSelect.playerTeam = team;

        // Spawn via network if online
        var spawner = FindFirstObjectByType<NetworkSpawner>();
        if (spawner != null)
        {
            spawner.SpawnPlayer(team);
        }

        EnablePlayerControls();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // No room found - create one
        CreateRoom("Room_" + Random.Range(1000, 9999));
    }
}
