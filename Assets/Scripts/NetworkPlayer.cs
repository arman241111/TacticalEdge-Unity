using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class NetworkPlayer : MonoBehaviourPun, IPunObservable
{
    [Header("Sync")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float lerpSpeed = 15f;

    [Header("State")]
    public string team = "ct";
    public int health = 100;
    public bool isDead = false;

    void Start()
    {
        if (photonView != null && !photonView.IsMine)
        {
            // Remote player - disable local controls
            DisableLocalControls();
        }
    }

    void DisableLocalControls()
    {
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        var ss = GetComponent<SimpleShoot>();
        if (ss != null) ss.enabled = false;

        var sc = GetComponent<SimpleCrosshair>();
        if (sc != null) sc.enabled = false;

        var wl = GetComponent<WeaponLoader>();
        if (wl != null) wl.enabled = false;

        var wcf = GetComponent<WeaponColorFixer>();
        if (wcf != null) wcf.enabled = false;

        // Disable camera for remote players
        var cam = GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;

        var listener = GetComponentInChildren<AudioListener>();
        if (listener != null) listener.enabled = false;
    }

    void Update()
    {
        if (photonView != null && !photonView.IsMine)
        {
            // Smooth interpolation for remote players
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * lerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * lerpSpeed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Local player sends data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(health);
            stream.SendNext(isDead);
            stream.SendNext(team);
        }
        else
        {
            // Remote player receives data
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            health = (int)stream.ReceiveNext();
            isDead = (bool)stream.ReceiveNext();
            team = (string)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage, int attackerViewID)
    {
        if (!photonView.IsMine) return;

        health -= damage;
        var ph = GetComponent<PlayerHealth>();
        if (ph != null) ph.currentHealth = health;

        if (health <= 0)
        {
            isDead = true;
            Die();
        }
    }

    [PunRPC]
    public void RPC_Shoot(Vector3 origin, Vector3 direction, int damage)
    {
        // Visual effect for remote player shooting
        // Can add muzzle flash here later
    }

    void Die()
    {
        // Notify RoundManager
        if (RoundManager.Instance != null)
        {
            // Player died
        }
    }

    public void ShootNetwork()
    {
        if (photonView == null || !photonView.IsMine) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 origin = cam.transform.position;
        Vector3 direction = cam.transform.forward;

        // Local raycast
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f))
        {
            // Hit another network player?
            var targetView = hit.collider.GetComponentInParent<PhotonView>();
            if (targetView != null && targetView != photonView)
            {
                var targetNP = targetView.GetComponent<NetworkPlayer>();
                if (targetNP != null && targetNP.team != team)
                {
                    // Get current weapon damage
                    var shoot = GetComponent<SimpleShoot>();
                    int dmg = shoot != null ? (int)shoot.damage : 25;
                    targetView.RPC("RPC_TakeDamage", RpcTarget.All, dmg, photonView.ViewID);
                }
            }
        }

        // Tell others about the shot (for visual effects)
        photonView.RPC("RPC_Shoot", RpcTarget.Others, origin, direction, 0);
    }
}
