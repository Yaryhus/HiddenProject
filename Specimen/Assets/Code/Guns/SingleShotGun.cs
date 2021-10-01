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
    [SerializeField] GameObject bullet;
    [SerializeField] List<GameObject> bullets;
    [SerializeField] Transform initialBulletPos;

    [Header("Sounds")]
    [SerializeField]
    Sound shootSound = null;
    [SerializeField]
    Sound reloadSound = null;
    [SerializeField]
    Sound emptySound = null;
    [SerializeField]
    Sound idleSound = null;
    [SerializeField]
    Sound aimSound = null;
    [SerializeField]
    Sound hideSound = null;
    [SerializeField]
    Sound showSound = null;

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

    public bool isAutomatic = false;
    //Reload
    private int currentAmmo, bulletShot;
    private WaitForSeconds reloadWait;
    private WaitForSeconds reloadWaitTransition;
    private bool isReloading = false;


    //Shoot automatic
    private float nextTimeToFire = 0f;
    public float fireRate = 0;
    [Header("Debug options to mess around")]
    [SerializeField]
    float maxAmmo;
    [SerializeField]
    bool infiniteAmmo = false;

    PhotonView PV;
    bool isShooting = false;

    int bulletsDropIndex = 0;

    void Start()
    {
        anim = itemGameObject.GetComponent<Animator>();
        isAutomatic = ((GunInfo)itemInfo).isAutomatic;
        maxAmmo = ((GunInfo)itemInfo).maxAmmo;
        fireRate = ((GunInfo)itemInfo).fireRate;
    }

    void Awake()
    {

        anim = itemGameObject.GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
        isAutomatic = ((GunInfo)itemInfo).isAutomatic;
        fireRate = ((GunInfo)itemInfo).fireRate;

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


    }

    public override void Reload()
    {
        //We are not reloading, we have less than the whole magazine and we have more than 0 bullets
        if (currentAmmo != ((GunInfo)itemInfo).magazineAmmo && !isReloading && maxAmmo > 0)
        {
            StartCoroutine(ReloadW());
        }
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
        if (currentAmmo == 0)
            emptySound.PlayOneShot(transform);

        if (!isShooting)
        {
            //Debug.Log("I shot");
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
            
            //Normally we would call this method in animations but since shot animations are so short it does skip sometimes the event call
            DetectDamage();          
        }
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
        anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_RELOAD);
        reloadSound.Play(transform);
        
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
    }

    public override void Aim()
    {
        aimSound.Play(transform);
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

    public override void DetectDamage()
    {
        Debug.Log("He disparado de verdad");
        //Ciclic loop for getting a different bullet everytime
        if (bulletsDropIndex == bullets.Capacity - 1)
        {
            bulletsDropIndex = 0;
        }
        else
        {
            bulletsDropIndex++;
        }
        //Throw Bullet
        ThrowBullet(bulletsDropIndex);


        //Spray for weapon
        Vector3 shootDirection = cam.transform.forward;
        shootDirection.x += Random.Range(-((GunInfo)itemInfo).spreadFactorX, ((GunInfo)itemInfo).spreadFactorX);
        shootDirection.y += Random.Range(-((GunInfo)itemInfo).spreadFactorY, ((GunInfo)itemInfo).spreadFactorY);


        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        Debug.DrawRay(ray.origin, shootDirection * ((GunInfo)itemInfo).range, Color.blue, 3.0f);
        if (Physics.Raycast(ray.origin, shootDirection, out RaycastHit hit))
        {
            //if hit is in range
            if (hit.distance <= ((GunInfo)itemInfo).range)
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
            else
            {
                Debug.Log("I missed, its too far");
            }
        }
    }
    void ThrowBullet(int index)
    {
        bullets[index].SetActive(true);
        bullets[index].GetComponent<Rigidbody>().AddForce(new Vector3(1, 0, 0) * 3.0f, ForceMode.Impulse);
        StartCoroutine(BulletReturn(bullets[index]));
    }

    IEnumerator BulletReturn(GameObject _bullet)
    {
        //Reset the bullet to their initial pos when 1 second passed.
        Rigidbody rigidbody = _bullet.GetComponent<Rigidbody>();
        yield return new WaitForSeconds(1.0f);
        //Hide and reset the bullet

        _bullet.SetActive(false);

        rigidbody.velocity = new Vector3(0f, 0f, 0f);
        rigidbody.angularVelocity = new Vector3(0f, 0f, 0f);

        _bullet.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        _bullet.transform.position = initialBulletPos.position;

    }
}