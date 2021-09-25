using EZCameraShake;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SingleShotGun : Gun
{
    [Header("Required Components")]
    [SerializeField] Camera cam;
    [SerializeField] Animator anim;


    [Header("Sounds")]
    [SerializeField]
    Sound shootSound = null;
    [SerializeField]
    Sound reloadSound = null;
    [SerializeField]
    Sound emptySound = null;
    [SerializeField]
    Sound idleSound = null;

    [Header("Particles")]
    [SerializeField]
    ParticleSystem muzzleFlash = null;

    [Header("Shake")]
    [SerializeField]
    [Tooltip("Suavidad del tiemble. Valores bajos son mas suaves")]
    float shakeRoughness = 1.0f;
    [SerializeField]
    [Tooltip("Intensidad del tiemble")]
    float shakeMagnitude = 1.0f;
    [SerializeField]
    [Tooltip("Cuanto tarda en ocurrir")]
    float shakeFadeInTime = 0.1f;
    [SerializeField]
    [Tooltip("Cuanto tarda en irse")]
    float shakeFadeOutTime = 1f;


    [Header("HUD")]
    [SerializeField]
    TextMeshProUGUI text;

    //Reload
    private int currentAmmo, bulletShot;
    private WaitForSeconds reloadWait;
    private WaitForSeconds reloadWaitTransition;
    private bool isReloading = false;


    //Shoot automatic
    private float nextTimeToFire = 0f;

    PhotonView PV;

    void Start()
    {
        anim = itemGameObject.GetComponent<Animator>();

    }

    void Awake()
    {
        anim = itemGameObject.GetComponent<Animator>();
        PV = GetComponent<PhotonView>();

        //Setting up ammo
        reloadWait = new WaitForSeconds(((GunInfo)itemInfo).reloadTime - .25f);
        reloadWaitTransition = new WaitForSeconds(.25f);
        currentAmmo = ((GunInfo)itemInfo).maxAmmo;
    }

    public override void Use()
    {
        //If gun is empty
        if (currentAmmo <= 0)
        {
            //Empty gun, reproduce sound/Anim
            emptySound.Play(transform);
        }

        //Shooting is automatic
        else if (((GunInfo)itemInfo).isAutomatic)
        {
            if (Time.time >= nextTimeToFire && currentAmmo > 0)
            {
                bulletShot = ((GunInfo)itemInfo).bulletsPerShot;
                nextTimeToFire = Time.time + 1f / ((GunInfo)itemInfo).fireRate;
                Shoot();
            }

        }
        //Shooting if semiautomatic
        else
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
        if (currentAmmo != ((GunInfo)itemInfo).maxAmmo && !isReloading)
        {
            StartCoroutine(ReloadW());
        }
    }

    //To update the ammo counter and weapon ammo
    public void UpdateText()
    {
        //update text
        text.SetText(currentAmmo + " / " + ((GunInfo)itemInfo).maxAmmo);
    }

    void Shoot()
    {

        currentAmmo--;
        bulletShot--;
        //Camera shake if any
        //StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
        CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, shakeFadeInTime, shakeFadeOutTime);

        //Shoot animation
        anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_SHOOT);

        //Sound
        shootSound.PlayOneShot(transform);

        //Muzzle Flash VFX
        if (muzzleFlash != null)
            muzzleFlash.Play();

        //Spray for weapon
        Vector3 shootDirection = cam.transform.forward;
        shootDirection.x += Random.Range(-((GunInfo)itemInfo).spreadFactorX, ((GunInfo)itemInfo).spreadFactorX);
        shootDirection.y += Random.Range(-((GunInfo)itemInfo).spreadFactorY, ((GunInfo)itemInfo).spreadFactorY);


        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray.origin, shootDirection, out RaycastHit hit))
        {
            //Impact the collider and add a force
            if (hit.rigidbody != null && !hit.transform.tag.Equals("Player"))
            {
                hit.rigidbody.AddForce(-hit.normal * ((GunInfo)itemInfo).impactForce);
            }

            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
            Debug.Log(hit.collider.gameObject.name);
        }

        if (bulletShot > 0 && currentAmmo > 0)
            Invoke("Shoot", 1 / ((GunInfo)itemInfo).fireRate);
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            //Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }

    IEnumerator ReloadW()
    {
        isReloading = true;
        reloadSound.Play(transform);
        anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_RELOAD);
        //wait Reload Time
        yield return reloadWait;
        //wait for the transition to end
        yield return reloadWaitTransition;
        currentAmmo = ((GunInfo)itemInfo).maxAmmo;
        isReloading = false;
    }

}