using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName = "AK-47";
    public float damage = 25f;
    public float fireRate = 0.1f;
    public float range = 100f;
    public float spread = 0.02f;
    public bool isAutomatic = true;
    public int maxAmmo = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 2.5f;

    [Header("References")]
    public Camera playerCamera;
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlash;
    public GameObject bulletHolePrefab;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Recoil")]
    public float recoilX = 1f;
    public float recoilY = 2f;
    public float recoilRecovery = 5f;

    [Header("State")]
    public int currentAmmo;
    public bool isReloading = false;

    private float nextFireTime = 0f;
    private float reloadTimer = 0f;
    private Mouse mouse;
    private Keyboard keyboard;

    void Start()
    {
        currentAmmo = maxAmmo;
        if (playerCamera == null)
            playerCamera = Camera.main;
        mouse = Mouse.current;
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (mouse == null) mouse = Mouse.current;
        if (keyboard == null) keyboard = Keyboard.current;
        if (mouse == null || keyboard == null) return;

        // Reloading
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                int ammoNeeded = maxAmmo - currentAmmo;
                int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
                currentAmmo += ammoToReload;
                reserveAmmo -= ammoToReload;
                isReloading = false;
            }
            return;
        }

        // Shooting
        if (isAutomatic)
        {
            if (mouse.leftButton.isPressed && Time.time >= nextFireTime)
                Shoot();
        }
        else
        {
            if (mouse.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
                Shoot();
        }

        // Reload
        if (keyboard.rKey.wasPressedThisFrame && currentAmmo < maxAmmo && reserveAmmo > 0 && !isReloading)
            StartReload();

        // Auto reload on empty
        if (currentAmmo <= 0 && reserveAmmo > 0 && !isReloading)
            StartReload();
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            if (audioSource && emptySound)
                audioSource.PlayOneShot(emptySound);
            return;
        }

        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        // Muzzle flash
        if (muzzleFlash != null)
            muzzleFlash.Play();

        // Sound
        if (audioSource && shootSound)
            audioSource.PlayOneShot(shootSound);

        // Raycast
        Vector3 shootDir = playerCamera.transform.forward;
        shootDir += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            Random.Range(-spread, spread)
        );

        Ray ray = new Ray(playerCamera.transform.position, shootDir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            // Try EnemyAI first
            EnemyAI enemyAI = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
            {
                float dmg = damage;
                if (hit.collider.CompareTag("Head"))
                    dmg *= 4f;
                enemyAI.TakeDamage(dmg);
            }
            // Fallback to EnemyHealth
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                float dmg = damage;
                if (hit.collider.CompareTag("Head"))
                    dmg *= 4f;
                enemy.TakeDamage(dmg);
            }

            if (bulletHolePrefab != null && enemy == null)
            {
                GameObject hole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                Destroy(hole, 5f);
            }
        }
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
        if (audioSource && reloadSound)
            audioSource.PlayOneShot(reloadSound);
    }
}
