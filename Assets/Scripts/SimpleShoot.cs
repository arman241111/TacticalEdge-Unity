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
        }
    }

    void Shoot()
    {
        nextFire = Time.time + fireRate;
        ammo--;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        Debug.Log("BANG! Ammo: " + ammo);

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Enemy hit! HP: " + enemy.health);
            }
        }
    }
}
