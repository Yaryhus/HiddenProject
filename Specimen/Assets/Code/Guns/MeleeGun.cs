using EZCameraShake;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeleeGun : Gun
{
    [Header("Required Components")]
    [SerializeField] Camera cam;
    [SerializeField] Animator anim;
    [SerializeField] LayerMask ignoreLayers;

    [Header("Sounds")]
    [SerializeField]
    Sound shootSound = null;
    [SerializeField]
    Sound specialAttackSound = null;
    [SerializeField]
    Sound idleSound = null;
    [SerializeField]
    Sound showOffSound = null;
    [SerializeField]
    Sound showSound = null;
    [SerializeField]
    Sound hideSound = null;

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
    bool isShooting = false;
    float damage = 0;
    PhotonView PV;

    void Start()
    {
        anim = itemGameObject.GetComponent<Animator>();


    }

    void Awake()
    {
        anim = itemGameObject.GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
        showSound.PlayOneShot(transform);
    }

    //Shoot or hit
    public override void Use()
    {
        Shoot(0);
    }

    //Instead of aiming, melee does special attack and double damage
    public override void Aim()
    {
        Shoot(1);
    }

    //To update the HUD if neccesary
    public void UpdateText()
    {
        text.SetText(" -  / - ");
    }

    void Shoot(int typeOfAttack)
    {
        if (!isShooting)
        {
            //Standard damage used for the attack. Will be increased if special attack
            damage = ((GunInfo)itemInfo).damage;

            //Camera shake if any
            //StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
            CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, shakeFadeInTime, shakeFadeOutTime);

            //Shoot animation
            if (typeOfAttack == 0)
            {
                anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_SHOOT);
                Debug.Log(damage);
                //Sound
                shootSound.PlayOneShot(transform);
            }
            else if (typeOfAttack == 1)
            {
                anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_SPECIALATTACK);
                damage = ((GunInfo)itemInfo).specialDamage;
                Debug.Log(damage);
                //Sound
                specialAttackSound.PlayOneShot(transform);
            }

            //here on after, the animation is the one responsible for detecting hits and bullet holes, on DetectDamage() below

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
    //Instead of reload, the melee launches a taunt
    public override void Reload()
    {
        anim.SetTrigger(GlobalVariablesAndStrings.ANIM1_TAUNT);
        showOffSound.PlayOneShot(transform);
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
        //Muzzle Flash VFX
        if (muzzleFlash != null)
            muzzleFlash.Play();

        //Spray for weapon
        Vector3 shootDirection = cam.transform.forward;
        //shootDirection.x += Random.Range(-((GunInfo)itemInfo).spreadFactorX, ((GunInfo)itemInfo).spreadFactorX);
        //shootDirection.y += Random.Range(-((GunInfo)itemInfo).spreadFactorY, ((GunInfo)itemInfo).spreadFactorY);


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

               //hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);
                hit.collider.transform.gameObject.GetComponentInParent<IDamageable>()?.TakeDamage(damage);
                PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
                Debug.Log(hit.collider.gameObject.name);

            }
            else
            {
                Debug.Log("I missed, its too far");
            }
        }
    }
}
