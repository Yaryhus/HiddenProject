using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo

{
    [Header("Basic Stats")]

    public float damage = 10f;

    public float range = 100f;

    public int bulletsPerShot = 1;

    [SerializeField]
    public int maxAmmo = 10;

    [Tooltip("Lo que varia el disparo en horizontal")]
    public float reloadTime = 1.2f;

    public float spreadFactorX = 0.0f;

    [Tooltip("Lo que varia el disparo en vertical")]

    public float spreadFactorY = 0.0f;

    public bool isAutomatic = false;

    [Tooltip("No se usa si el arma es semiauomática")]
    public float fireRate = 15f;

    [Tooltip("Fuerza con la que mueve el ente impactado")]
    public float impactForce = 30f;
}
