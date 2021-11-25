using EZCameraShake;
using Photon.Pun;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SingleShotGun : Gun
{
    [Header("Required Components")]
    [SerializeField] Camera cam;
    [SerializeField] Animator anim;
    [SerializeField] GameObject thirdPersonAnimObject;
    [SerializeField] Animator thirdPersonAnim;
    [SerializeField] Transform initialBulletPos;
    [SerializeField] LayerMask ignoreLayers;

    //Drop bullets from a pool of bullets from certain prefab
    [ValueDropdown("TypeOfBullet")]
    [SerializeField] string typeOfBullet;
    string[] TypeOfBullet = { "RifleBullets", "ShotgunBullets", "PistolBullets" };
    ObjectPooler objectPooler;


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
    [SerializeField]
    ParticleSystem bulletDrop = null;

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

    [Header("Debug options to mess around")]
    [SerializeField]
    float maxAmmo;
    [SerializeField]
    bool infiniteAmmo = false;

    //Reload
    private int currentAmmo, bulletShot;
    private WaitForSeconds reloadWait;
    private WaitForSeconds reloadWaitTransition;
    private bool isReloading = false;

    //Shoot automatic
    private float nextTimeToFire = 0f;
    public float fireRate = 0;
    bool isAutomatic = false;

    //Shoot and aim
    bool isShooting = false;
    float spreadFactorX;
    float spreadFactorY;
    bool aiming = false;


    //Photon and MP components
    PhotonView PV;

    void Start()
    {
        anim = itemGameObject.GetComponent<Animator>();
        isAutomatic = ((GunInfo)itemInfo).isAutomatic;
        maxAmmo = ((GunInfo)itemInfo).maxAmmo;
        fireRate = ((GunInfo)itemInfo).fireRate;
        spreadFactorX = ((GunInfo)itemInfo).spreadFactorX;
        spreadFactorY = ((GunInfo)itemInfo).spreadFactorY;
        bulletShot = ((GunInfo)itemInfo).bulletsPerShot;
        thirdPersonAnim = thirdPersonAnimObject.GetComponentInChildren<Animator>();


        //Bullets pool
        objectPooler = ObjectPooler.Instance;

    }

    private void OnEnable()
    {
        //thirdPersonAnim.GetLayerIndex()
    }

    private void OnDisable()
    {

    }

    void Awake()
    {

        anim = itemGameObject.GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
        isAutomatic = ((GunInfo)itemInfo).isAutomatic;
        fireRate = ((GunInfo)itemInfo).fireRate;
        bulletShot = ((GunInfo)itemInfo).bulletsPerShot;
        //Setting up ammo
        reloadWait = new WaitForSeconds(((GunInfo)itemInfo).reloadTime - .25f);
        reloadWaitTransition = new WaitForSeconds(.25f);
        currentAmmo = ((GunInfo)itemInfo).magazineAmmo;
        showSound.PlayOneShot(transform);

        objectPooler = ObjectPooler.Instance;
    }

    public bool GetIsAutomatic()
    {
        return isAutomatic;
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

            //Shooting
            if (currentAmmo > 0)
            {
                Shoot();
            }
        }


    }



    //To update the ammo counter and weapon ammo
    public override void UpdateText()
    {
        //update text. If max size is below 0 (should not happen), it resets to 0.
        if (maxAmmo < 0)
            maxAmmo = 0;

        text.SetText(currentAmmo + " / " + maxAmmo);
    }

    void Shoot()
    {
        Debug.Log("I shoot");
        //Camera shake if any
        //StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
        CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, shakeFadeInTime, shakeFadeOutTime);

        //Shoot animation
        if (currentAmmo == 1)
            anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_SHOOTLAST);
        else
            anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_SHOOT);

        //Sound
        shootSound.PlayOneShot(transform);

        //Muzzle Flash VFX
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        currentAmmo -= bulletShot;

        //Normally we would call this method in animations but since shot animations are so short it does skip sometimes the event call
        DetectDamage();
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal, System.String sharedM)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            //Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
            if (colliders[0].GetComponent<Collider>().sharedMaterial != null)
            {
                //Debug.Log(colliders[0].GetComponent<Collider>().sharedMaterial);
                bulletImpactObj.GetComponent<BulletImpact>().ChangeVisuals(sharedM);
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
        /*
        //If we are aiming we stop aiming, and viceversa. WE also are more accureate while aiming, but slower moving.
        if (!aiming)
        {
            aiming = true;
            aimSound.Play(transform);
            anim.SetBool(GlobalVariablesAndStrings.ANIM1_BOOLEAN_AIM, true);

            spreadFactorX = spreadFactorX / 2;
            spreadFactorY = spreadFactorY / 2;
            GetComponentInParent<FPSMovementController>().DecreaseSpeed(2.0f);
        }
        else
        {
            aiming = false;
            aimSound.Play(transform);
            anim.SetBool(GlobalVariablesAndStrings.ANIM1_BOOLEAN_AIM, false);
            spreadFactorX = ((GunInfo)itemInfo).spreadFactorX;
            spreadFactorY = ((GunInfo)itemInfo).spreadFactorY;
            GetComponentInParent<FPSMovementController>().IncreaseSpeed(2.0f);

        }
        */
    }

    //TO DO - CLEAN THIS METHODS AND THE ANIMATION EVENTS INSIDE THE RIFLE/PISTOL ANIMATOR
    //For animation events
    public override void AttackStart()
    {
        //isShooting = true;
    }

    public override void AttackEnd()
    {
        //isShooting = false;
    }

    public override void DetectDamage()
    {
        //Debug.Log("He disparado de verdad");

        //Throw Bullet prefab (from pool)
        ThrowBullet();

        //Spray for weapon
        Vector3 shootDirection = cam.transform.forward;
        shootDirection.x += Random.Range(-((GunInfo)itemInfo).spreadFactorX, ((GunInfo)itemInfo).spreadFactorX);
        shootDirection.y += Random.Range(-((GunInfo)itemInfo).spreadFactorY, ((GunInfo)itemInfo).spreadFactorY);


        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        Debug.DrawRay(ray.origin, shootDirection * ((GunInfo)itemInfo).range, Color.blue, 3.0f);
        if (Physics.Raycast(ray.origin, shootDirection, out RaycastHit hit, ((GunInfo)itemInfo).range, ~ignoreLayers))
        {
            //if hit is in range
            if (hit.distance <= ((GunInfo)itemInfo).range)
            {
                //Impact the collider and add a force
                if (hit.rigidbody != null && !hit.transform.tag.Equals("Player"))
                {
                    hit.rigidbody.AddForce(-hit.normal * ((GunInfo)itemInfo).impactForce);
                }

                float _damage = ((GunInfo)itemInfo).damage;

                //Apply damage to players
                if (hit.collider.gameObject.GetComponent<HitBoxBodyPart>())
                {
                    //Debug.Log("Detected a body part");
                    string bodypart = hit.collider.gameObject.GetComponent<HitBoxBodyPart>().GetBodyPartType();
                    // Debug.Log(bodypart);

                    switch (bodypart)
                    {
                        case "ArmsOrLegs":
                            _damage = _damage * GlobalVariablesAndStrings.DAMAGE_MULT_ARMORLEG;
                            break;
                        case "Torso":
                            _damage = _damage * GlobalVariablesAndStrings.DAMAGE_MULT_TORSO;
                            break;
                        case "Head":
                            _damage = _damage * GlobalVariablesAndStrings.DAMAGE_MULT_HEAD;
                            break;
                        case "null":
                            break;

                    }
                }
                //Debug.Log("I hit " + hit.collider.gameObject.name + " with layer " + hit.collider.gameObject.layer + " and its material is " + hit.collider.sharedMaterial.name + " and with this damage " + _damage);
                //We apply the damage to the owner of the body if any
                hit.collider.transform.gameObject.GetComponentInParent<IDamageable>()?.TakeDamage(_damage);

                // if (hit.collider.sharedMaterial !=null)
                // {
                PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal, hit.collider.sharedMaterial?.name);

                // }
                //else
                // PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);

            }
            else
            {
                Debug.Log("I missed, its too far");
            }
        }
    }

    //Throw bullet prefab from pool
    void ThrowBullet()
    {
        if (bulletDrop != null)
        {
            bulletDrop.Play();
        }

        //objectPooler.SpawnFromPool(typeOfBullet, initialBulletPos.position, initialBulletPos.rotation);

    }

    //Reset values and stops coroutines like reloading
    public override void OnChangeWeapon()
    {
        //StartCoroutine(ChangeWeaponAnimation());
        isReloading = false;
        StopAllCoroutines();
    }

    private IEnumerator ChangeWeaponAnimation()
    {
        anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_HIDEWEAPON);
        float length;
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        length = info.length;

        yield return new WaitForSeconds(length);
        isReloading = false;
        StopAllCoroutines();

    }
}