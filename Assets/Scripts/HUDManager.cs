using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("References")]
    public WeaponSystem weapon;
    public PlayerHealth playerHealth;

    [Header("UI Elements")]
    public Text ammoText;
    public Text healthText;
    public Text weaponNameText;
    public Image crosshairDot;
    public Image crosshairTop;
    public Image crosshairBottom;
    public Image crosshairLeft;
    public Image crosshairRight;

    void Update()
    {
        if (weapon != null)
        {
            if (ammoText != null)
            {
                string reloadText = weapon.isReloading ? " [R]" : "";
                ammoText.text = weapon.currentAmmo + " / " + weapon.reserveAmmo + reloadText;
            }
            if (weaponNameText != null)
                weaponNameText.text = weapon.weaponName;
        }

        if (playerHealth != null && healthText != null)
        {
            healthText.text = "HP: " + playerHealth.currentHealth;
        }
    }
}
