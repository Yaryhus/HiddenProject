using UnityEngine;
using System.Collections;
using EZCameraShake;
using TMPro;

public class Gun : MonoBehaviour
{
    [Header("Basic Stats")]

    [SerializeField]
    float damage = 10f;

    [SerializeField]
    float range = 100f;

    [SerializeField]
    int bulletsPerShot = 1;

    [SerializeField]
    int maxAmmo = 10;

    [SerializeField]
    [Tooltip("Lo que varia el disparo en horizontal")]
    float reloadTime = 1.2f;

    [SerializeField]
    float spreadFactorX = 0.0f;
    [SerializeField]
    [Tooltip("Lo que varia el disparo en vertical")]

    float spreadFactorY = 0.0f;

    [SerializeField]
    bool isAutomatic = false;

    [SerializeField]
    [Tooltip("No se usa si el arma es semiauomática")]
    float fireRate = 15f;

    [SerializeField]
    [Tooltip("Fuerza con la que mueve el ente impactado")]
    float impactForce = 30f;

    [Header("Animator and Animations")]
    [SerializeField]
    Animator gunAnim = null;

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
    [SerializeField]
    GameObject impactEffect = null;
    
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
    
    [Header("Required Components")]
    [SerializeField]
    Camera cam = null;
    [SerializeField]
    LayerMask whatIsEnemy;
    
    [Header("HUD")]
    [SerializeField]
    TextMeshProUGUI text;

    GameObject player;
    private float nextTimeToFire = 0f;
    //Reload
    private int currentAmmo, bulletShot;
    private WaitForSeconds reloadWait;
    private WaitForSeconds reloadWaitTransition;

    private bool isReloading = false;
    FPSMovementController playerController;
    void Start()
    {
        //Get player controller
        player = PlayerManager.instance.Player;
        playerController = player.GetComponent<FPSMovementController>();
        //Setting up ammo
        reloadWait = new WaitForSeconds(reloadTime - .25f);
        reloadWaitTransition = new WaitForSeconds(.25f);
        currentAmmo = maxAmmo;

        //Sound
        emptySound.Init();
        shootSound.Init();
        reloadSound.Init();
        idleSound.Init();
    }

    //If user switches weapons
    void OnEnable()
    {
        isReloading = false;
        gunAnim.SetBool("Reloading", false);
    }

    // Update is called once per frame
    void Update()
    {

        //Reload when pressed Reload button
        if (Input.GetButtonDown("Reload") && currentAmmo != maxAmmo && !isReloading)
        {
            StartCoroutine(Reload());
        }
        //if the user is reloading, do nothing.
        if (isReloading)
        {
            return;
        }

        //Movement animation
        if (playerController.GetplayerIsMoving())
        {
            //Debug.Log("User is moving");
            gunAnim.SetBool("isMoving", true);
        }
        else if (!playerController.GetplayerIsMoving())
        {
            gunAnim.SetBool("isMoving", false);
        }

        //If we are empty.
        if (currentAmmo <= 0 && Input.GetButtonDown("Shoot"))
        {
            //Empty Sound
            emptySound.Play(transform);
            //Optional: Reload when empty
            //StartCoroutine(Reload());
            //return;
        }


        if (isAutomatic)
        {
            //Shooting if automatic
            if (Input.GetButton("Shoot") && Time.time >= nextTimeToFire && currentAmmo > 0)
            {
                bulletShot = bulletsPerShot;
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }

        }
        else
        {
            //Shooting if semiautomatic
            if (Input.GetButtonDown("Shoot") && currentAmmo > 0)
            {
                bulletShot = bulletsPerShot;
                Shoot();
            }

        }
        //update text
        text.SetText(currentAmmo + " / " + maxAmmo);
    }
    IEnumerator Reload()
    {
        isReloading = true;
        reloadSound.Play(transform);
        gunAnim.SetBool("Reloading", true);
        //wait Reload Time
        yield return reloadWait;
        gunAnim.SetBool("Reloading", false);
        //wait for the transition to end
        yield return reloadWaitTransition;
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void Shoot()
    {
        currentAmmo--;
        bulletShot--;
        //Camera shake if any
        //StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
        CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, shakeFadeInTime, shakeFadeOutTime);
        //Shooting anim
        gunAnim.SetTrigger("Shoot");

        //Sound
        shootSound.PlayOneShot(transform);
        //Muzzle Flash VFX
        if (muzzleFlash != null)
            muzzleFlash.Play();

        Vector3 shootDirection = cam.transform.forward;
        shootDirection.x += Random.Range(-spreadFactorX, spreadFactorX);
        shootDirection.y += Random.Range(-spreadFactorY, spreadFactorY);
        //Check the ray up to the range determined.
        //Debug.DrawRay(cam.transform.position, ShootDirection,Color.blue, 10.0f);

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, shootDirection, out hit, whatIsEnemy))
        {
            Debug.Log(hit.transform.name);

            /*
            //If something is impacted. Now we check if its the enemy 
            EnemyBehavior enemy = hit.transform.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            */

            //Impact the collider and add a force
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            //Impact effect on whatever shot
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 1.0f);
            }

        }
        if (bulletShot > 0 && currentAmmo > 0)
            Invoke("Shoot", 1 / fireRate);
    }
}
