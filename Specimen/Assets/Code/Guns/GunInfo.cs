using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo

{
    [Header("Basic Stats")]

    public float damage = 10f;

    [ShowIf("isMelee")]
    public float specialDamage = 30f;

    public float range = 100f;

    public bool isMelee = false;

    [HideIf("isMelee")]
    public int bulletsPerShot = 1;

    [HideIf("isMelee")]
    [SerializeField]
    public int magazineAmmo = 10;
    [HideIf("isMelee")]
    [SerializeField]
    public int maxAmmo = 100;

    [HideIf("isMelee")]
    [Tooltip("Lo que varia el disparo en horizontal")]
    public float reloadTime = 1.2f;
    [HideIf("isMelee")]
    public float spreadFactorX = 0.0f;

    [HideIf("isMelee")]
    [Tooltip("Lo que varia el disparo en vertical")]
    public float spreadFactorY = 0.0f;

    [HideIf("isMelee")]
    public bool isAutomatic = false;

    [HideIf("@this.isMelee || !isAutomatic")]
    [Tooltip("No se usa si el arma es semiauomática")]
    public float fireRate = 15f;

    [Tooltip("Fuerza con la que mueve el ente impactado")]
    public float impactForce = 30f;
}
