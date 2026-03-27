using UnityEngine;
using UnityEngine.InputSystem;

public class GameSetup : MonoBehaviour
{
    void Start()
    {
        // Setup weapon system on player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            WeaponSystem ws = player.GetComponent<WeaponSystem>();
            if (ws == null)
                ws = player.AddComponent<WeaponSystem>();
            ws.playerCamera = Camera.main;
            ws.weaponName = "Glock-18";
            ws.damage = 25f;
            ws.fireRate = 0.15f;
            ws.isAutomatic = false;
            ws.maxAmmo = 20;
            ws.currentAmmo = 20;
            ws.reserveAmmo = 60;
            ws.range = 80f;
            ws.spread = 0.02f;
        }

        // HUD is handled by SimpleCrosshair.cs and RoundManager.cs
        // No need to create Canvas here
    }
}
