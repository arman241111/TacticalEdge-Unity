using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleShoot : MonoBehaviour
{
    public float damage = 25f;
    public float range = 100f;
    public int ammo = 20;
    public int maxAmmo = 20;
    public int reserve = 60;
    public float fireRate = 0.15f;
    public bool isReloading = false;

    private float nextFire = 0f;
    private float reloadTimer = 0f;
    private Camera cam;
    private Mouse mouse;
    private Keyboard keyboard;

    void Start()
    {
        cam = Camera.main;
        mouse = Mouse.current;
        keyboard = Keyboard.current;
        Debug.Log("SimpleShoot ready! Camera: " + (cam != null));
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (mouse == null) mouse = Mouse.current;
        if (keyboard == null) keyboard = Keyboard.current;
        if (cam == null || mouse == null || keyboard == null) return;

        // Reload
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                int need = maxAmmo - ammo;
                int take = Mathf.Min(need, reserve);
                ammo += take;
                reserve -= take;
                isReloading = false;
            }
            return;
        }

        // Shoot with left mouse
        if (mouse.leftButton.isPressed && Time.time >= nextFire && ammo > 0)
        {
            Shoot();
        }

        // Reload with R
        if (keyboard.rKey.wasPressedThisFrame && ammo < maxAmmo && reserve > 0)
        {
            isReloading = true;
            reloadTimer = 2f;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayReload();
        }
    }

    void Shoot()
    {
        nextFire = Time.time + fireRate;
        ammo--;

        // Sound
        if (SoundManager.Instance != null)
        {
            string weaponType = "rifle";
            if (damage <= 20) weaponType = "pistol";
            else if (damage <= 25 && maxAmmo >= 25) weaponType = "smg";
            else if (damage >= 70) weaponType = "sniper";
            else if (maxAmmo <= 8) weaponType = "shotgun";
            else if (maxAmmo >= 100) weaponType = "heavy";
            SoundManager.Instance.PlayShoot(weaponType);
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            // Hit enemy AI
            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                bool wasAlive = !enemy.isDead;
                enemy.TakeDamage(damage);
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayHitBody();
                    if (enemy.isDead && wasAlive) SoundManager.Instance.PlayKillConfirm();
                }
                // Hitmarker
                if (GamePolish.Instance != null)
                {
                    GamePolish.Instance.ShowHitmarker(false);
                    if (enemy.isDead && wasAlive)
                    {
                        var rm = RoundManager.Instance;
                        string wep = rm != null ? rm.currentWeaponName : "Gun";
                        GamePolish.Instance.AddKillFeed("You", "Bot", wep, false);
                    }
                }
            }
            else
            {
                // Hit wall
                if (SoundManager.Instance != null) SoundManager.Instance.PlayHitWall();
            }

            // Hit network player
            var np = GetComponent<NetworkPlayer>();
            if (np != null) np.ShootNetwork();
        }
    }
}
