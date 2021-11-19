using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo

{
    [Header("Basic Stats")]

    public float damage = 10f;

    public float range = 100f;

    [Tooltip("Fuerza con la que mueve el ente impactado")]
    public float impactForce = 30f;

    [Header("Shotgun")]
    public bool isShotgun = false;
    [ShowIf("isShotgun")]
    public int pelletsPerShot = 5;

    [Header("Melee")]
    public bool isMelee = false;
    [ShowIf("isMelee")]
    public float specialDamage = 30f;


    [Header("Grenades and Throwables")]
    public bool isThrowable = false;
    [ShowIf("isThrowable")]
    public float delay = 3f;
    [ShowIf("isThrowable")]
    public float radius = 20f;

    [HideIf("isMelee")]
    [Header("Fire Guns")]
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
    [HideIf("@this.isMelee || isThrowable")]
    public float spreadFactorX = 0.0f;

    [HideIf("@this.isMelee || isThrowable")]
    [Tooltip("Lo que varia el disparo en vertical")]
    public float spreadFactorY = 0.0f;

    [HideIf("@this.isMelee || isThrowable")]
    public bool isAutomatic = false;

    [HideIf("@this.isMelee || !isAutomatic")]
    [Tooltip("No se usa si el arma es semiauomática")]
    public float fireRate = 15f;

}
