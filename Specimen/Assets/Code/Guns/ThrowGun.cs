using EZCameraShake;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class ThrowGun : Gun
{
    [Header("Required Components")]
    [SerializeField] Camera cam;
    [SerializeField] Animator anim;
    [SerializeField] Transform throwPoint;

    [Header("Sounds")]
    [SerializeField]
    Sound throwSound = null;
    [SerializeField]
    Sound idleSound = null;
    [SerializeField]
    Sound showSound = null;

    [Header("Particles")]
    [SerializeField]
    ParticleSystem muzzleFlash = null;


    [Header("HUD")]
    [SerializeField]
    TextMeshProUGUI text;

    //Reload
    private int currentAmmo, bulletShot;
    private WaitForSeconds reloadWait;
    private WaitForSeconds reloadWaitTransition;
    private bool isReloading = false;

    [Header("Debug options to mess around")]
    [SerializeField]
    int maxAmmo;
    [SerializeField]
    bool infiniteAmmo = false;

    GameObject grenade;

    PhotonView PV;
    bool isShooting = false;

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetTotalAmmo()
    {
        return maxAmmo;
    }

    void Start()
    {
        anim = itemGameObject.GetComponent<Animator>();

        maxAmmo = ((GunInfo)itemInfo).maxAmmo;

    }

    void Awake()
    {
        anim = itemGameObject.GetComponent<Animator>();
        PV = GetComponent<PhotonView>();

        //Setting up ammo
        reloadWait = new WaitForSeconds(((GunInfo)itemInfo).reloadTime - .25f);
        reloadWaitTransition = new WaitForSeconds(.25f);
        currentAmmo = ((GunInfo)itemInfo).magazineAmmo;
        showSound.PlayOneShot(transform);

    }

    public override void Use()
    {
        if (!isReloading)
        {
            if (currentAmmo > 0)
            {
                bulletShot = ((GunInfo)itemInfo).bulletsPerShot;
                Shoot();
            }
        }
    }

    public override void Reload()
    {
        StartCoroutine(ReloadW());
    }

    //To update the ammo counter and weapon ammo
    public void UpdateText()
    {
        //update text. If max size is below 0 (should not happen), it resets to 0.
        if (maxAmmo < 0)
            maxAmmo = 0;

        text.SetText(currentAmmo + " / " + maxAmmo);
    }

    void Shoot()
    {        

        if (!isShooting)
        {
            currentAmmo--;
            bulletShot--;

            //Shoot animation
            anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_SHOOT);

            //Animation drops the grenade and does the sound
            
            //We autoreload
            Reload();
            //Debug.Log("He terminado de disparar");
        }
    }


    IEnumerator ReloadW()
    {
        if (!isReloading)
        {

            isReloading = true;
            //reloadSound.Play(transform);
            yield return new WaitForSeconds(1.20f);
            anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_SHOWWEAPON);
            //wait Reload Time
            yield return reloadWait;
            //wait for the transition to end
            yield return reloadWaitTransition;

            currentAmmo = ((GunInfo)itemInfo).magazineAmmo;

            //If we dont have the infinite ammo debug selected
            if (!infiniteAmmo)
            {
                //We throw the whole mag when reloading
                maxAmmo -= ((GunInfo)itemInfo).magazineAmmo;
            }

            isReloading = false;
            Debug.Log("He recargado");
            isShooting = false;
        }
    }

    public override void Aim()
    {

    }

    //For animation events
    public override void AttackStart()
    {
        isShooting = true;
    }

    public override void AttackEnd()
    {
        isShooting = false;
    }

    //We use Detect Damage to spawn the grenade at the right moment
    public override void DetectDamage()
    {
        //Sound
        throwSound.PlayOneShot(transform);

        grenade = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "GrenadePrefab"), throwPoint.position, throwPoint.rotation, 0, new object[] { PV.ViewID });

        //If component is grenade we add the variables
        if (grenade.GetComponent<GrenadeObject>())
        {
            grenade.GetComponent<GrenadeObject>().delay = ((GunInfo)itemInfo).delay;
            grenade.GetComponent<GrenadeObject>().explosionForce = ((GunInfo)itemInfo).impactForce;
            grenade.GetComponent<GrenadeObject>().radius = ((GunInfo)itemInfo).radius;
            grenade.GetComponent<GrenadeObject>().damage = ((GunInfo)itemInfo).damage;
        }
        //we launch it
        grenade.GetComponent<Rigidbody>().AddForce(throwPoint.forward * ((GunInfo)itemInfo).range, ForceMode.Impulse);
    }
}