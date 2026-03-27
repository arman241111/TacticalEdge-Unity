using UnityEngine;
using Photon.Pun;

public class NetworkSpawner : MonoBehaviourPunCallbacks
{
    public static NetworkSpawner Instance;
    private GameObject localPlayer;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnPlayer(string team)
    {
        // Get spawn position based on team
        Vector3 spawnPos;
        var rm = FindFirstObjectByType<RoundManager>();
        if (rm != null)
        {
            spawnPos = team == "ct" ? rm.ctSpawnCenter : rm.tSpawnCenter;
            spawnPos += new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));

            // Raycast to find floor
            if (Physics.Raycast(spawnPos + Vector3.up * 50, Vector3.down, out RaycastHit hit, 100f))
            {
                spawnPos = hit.point + Vector3.up * 1.5f;
            }
        }
        else
        {
            spawnPos = new Vector3(0, 5, 0);
        }

        // Spawn network player
        if (PhotonNetwork.IsConnected && !PhotonNetwork.OfflineMode)
        {
            localPlayer = PhotonNetwork.Instantiate("NetworkPlayerPrefab", spawnPos, Quaternion.identity);
        }
        else
        {
            // Offline mode - just move existing player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = spawnPos;
                if (cc != null) cc.enabled = true;
            }
        }

        Debug.Log("Player spawned at " + spawnPos + " (team: " + team + ")");
    }
}
