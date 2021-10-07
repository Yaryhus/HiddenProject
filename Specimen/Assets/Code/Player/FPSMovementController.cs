using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using EZCameraShake;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;
using System.IO;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Rigidbody))]
public class FPSMovementController : MonoBehaviourPunCallbacks, IDamageable
{
    //Public Variables
    [Header("Main Variables")]

    [Header("Hidden Options")]
    [SerializeField]
    bool isHidden = false;

    [Header("Main Movement")]
    [SerializeField]
    float speed = 10.0f;
    float currentSpeed;
    [SerializeField]
    float sprintSpeed = 20.0f;
    bool isSprinting = false;

    [SerializeField]
    bool canMove = true;

    [Header("Jump")]
    [SerializeField]
    float jumpHeight = 3.0f;
    [ShowIf("isHidden")]
    [SerializeField]
    float leapForce = 20.0f;

    [Header("Health and Stamina Stats")]
    [SerializeField]
    float maxHealth = 100f;
    float currentHealth;

    [SerializeField]
    float maxStamina = 100f;
    [SerializeField]
    float sprintStaminaCost = 2.0f;
    [SerializeField]
    float jumpStaminaCost = 10f;
    float currentStamina;
    float lastRegen;
    [SerializeField]
    float staminaRegenSpeed = 1.0f;
    [SerializeField]
    float staminaRegenAmmount = 5.0f;

    [SerializeField]
    Image healthbarImage;
    [SerializeField]
    Image staminabarImage;
    bool isDead = false;


    [Header("Gravity")]
    [SerializeField]
    float gravity = -9.81f;
    [SerializeField]
    float groundDistance = 0.4f;
    [SerializeField]
    Transform groundCheck = null;
    [SerializeField]
    LayerMask groundMask;

    [Header("Sound")]
    [SerializeField]
    Sound walkSound = null;
    [SerializeField]
    float timeBetweenSteps = 0.2f;
    [SerializeField]
    Sound jumpSound = null;
    [SerializeField]
    Sound hurtSound = null;
    [SerializeField]
    Sound deadSound = null;

    [Header("Weapons and Items")]
    [SerializeField]
    List<Item> items;

    int itemIndex;
    int previousItemIndex = -1;

    [Header("UI")]
    [SerializeField]
    GameObject ui;

    PlayerManager playerManager;

    [Header("Animations")]
    [SerializeField]
    Animator thirdPersonAnimator;
    Animator firstPersonAnimator;

    [Header("Body for third person")]
    [SerializeField]
    GameObject userBody;
    [SerializeField]
    GameObject firsPersonWeapons;

    //Private variables
    Vector3 velocity;
    bool isGrounded;
    CharacterController controller;
    Rigidbody body;
    bool playerIsMoving = false;

    Transform objectInteracted;

    PhotonView PV;

    public Transform cam;

    InteractManager interactManager;

    //Shooting variables needeed here because of the Update Method.
    private float nextTimeToFire = 0f;

    public bool IsInteracting { get; private set; }

    void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;
        currentHealth = maxHealth;
        currentSpeed = speed;
        currentStamina = maxStamina;

        healthbarImage.fillAmount = currentHealth / maxHealth;
        staminabarImage.fillAmount = currentStamina / maxStamina;
        controller = GetComponent<CharacterController>();
        PV = GetComponent<PhotonView>();
        body = GetComponent<Rigidbody>();
        GlobalVariablesAndStrings.PLAYER = transform;
        interactManager = GetComponent<InteractManager>();

        //Find what body are we using to get the animator
        thirdPersonAnimator = body.GetComponentInChildren<Animator>();



        if (PV.IsMine)
        {
            EquipItem(0);
            //For now, deactivate 3rd person view (Will have to change this later so the user can see their feet or have shadows)
            userBody.SetActive(false);
        }
        else
        {
            Destroy(cam.gameObject);
            Destroy(body);
            Destroy(ui);
            controller.enabled = false;
            firsPersonWeapons.SetActive(false);
        }


        jumpSound.Init();
        walkSound.Init();

        InvokeRepeating("CallFootsteps", 0, timeBetweenSteps);
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        PV = GetComponent<PhotonView>();
        body = GetComponent<Rigidbody>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        cam = GetComponentInChildren<Camera>().transform;
        currentHealth = maxHealth;
        currentSpeed = speed;
        currentStamina = maxStamina;

        healthbarImage.fillAmount = currentHealth / maxHealth;
        staminabarImage.fillAmount = currentStamina / maxStamina;
        GlobalVariablesAndStrings.PLAYER = transform;
        interactManager = GetComponent<InteractManager>();

        //Find what body are we using to get the animator
        thirdPersonAnimator = body.GetComponentInChildren<Animator>();


        jumpSound.Init();
        walkSound.Init();

        InvokeRepeating("CallFootsteps", 0, timeBetweenSteps);
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine)
            return;

        //Movement
        Move();
        Sprint();
        Crouch();
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (Input.GetButtonDown("Leap") && isHidden && isGrounded && currentStamina >= jumpStaminaCost)
        {
            Leap();
        }

        RegenStamina();

        //Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Inventory management 
        ManageInventory();

        //UI management
        UpdateAmmoText();

        //Shooting and item usage
        //Shoot();

        //Shooting while not carrying an object
        if (items[itemIndex].GetComponent<SingleShotGun>() != null && items[itemIndex].GetComponent<SingleShotGun>().GetIsAutomatic() && Input.GetButton("Shoot") && interactManager.isHolding.Equals(false))
        {
            ShootAuto();
        }
        else if (Input.GetButtonDown("Shoot") && interactManager.isHolding.Equals(false))
        {
            ShootSemi();
        }


        //Shooting while carrying an object attachs it to the wall
        if (Input.GetButtonDown("Shoot") && interactManager.isHolding.Equals(true))
        {
            interactManager.AttachToObject();
        }

        //Aim
        if (Input.GetButtonDown("Aim"))
        {
            Aim();
        }

        if (Input.GetButtonUp("Aim"))
        {
            //StopAim();
        }
        Reload();

        //falling off the map
        if (transform.position.y < -100.0f)
        {
            Die();
        }

        if (Input.GetButtonDown("Interact"))
        {
            interactManager.PickUpObject();
        }

    }

    private void UpdateAmmoText()
    {
        //update text for ammo
        if (items[itemIndex].GetComponent<SingleShotGun>() != null)
        {
            ((SingleShotGun)items[itemIndex]).UpdateText();
        }
        else if (items[itemIndex].GetComponent<ThrowGun>() != null)
        {
            ((ThrowGun)items[itemIndex]).UpdateText();
        }
        else if (items[itemIndex].GetComponent<MeleeGun>() != null)
        {
            ((MeleeGun)items[itemIndex]).UpdateText();
        }
    }

    private void Reload()
    {
        //Reload item (weapon)
        if (Input.GetButtonDown("Reload"))
        {
            if (items[itemIndex].GetComponent<ThrowGun>() == null)
                ((Gun)items[itemIndex]).Reload();
        }
    }

    private void Aim()
    {
        ((Gun)items[itemIndex]).Aim();
    }

    private void ShootSemi()
    {
        items[itemIndex].Use();
        //Debug.Log("Disparo semi");
    }

    private void ShootAuto()
    {
        //Debug.Log("Disparo auto");

        //Particular case: Automatic fire
        if (items[itemIndex].GetComponent<SingleShotGun>() != null && items[itemIndex].GetComponent<SingleShotGun>().GetIsAutomatic())
        {
            if (Time.time - nextTimeToFire > 1 / items[itemIndex].GetComponent<SingleShotGun>().fireRate)
            {
                nextTimeToFire = Time.time;
                items[itemIndex].Use();
                //Debug.Log("Disparo");
            }
        }

    }

    void Sprint()
    {
        //Sprint
        if (Input.GetButton("Sprint") && currentStamina >= sprintStaminaCost)
        {
            firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 2f);
            currentSpeed = sprintSpeed;
            currentStamina -= sprintStaminaCost * Time.deltaTime;
            isSprinting = true;
            //Debug.Log("Sprinting");
            CheckStamina();
        }
        if (Input.GetButtonUp("Sprint"))
        {
            firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 1f);
            currentSpeed = speed;
            isSprinting = false;
            //Debug.Log("Stopped Sprinting");
            CheckStamina();
        }
    }

    void Crouch()
    {
        //Crouch
        if (Input.GetButtonDown("Crouch"))
        {
            controller.height = 1.0f;
        }
        if (Input.GetButtonUp("Crouch"))
        {
            controller.height = 1.9f;
        }
    }

    void ManageInventory()
    {
        //Switching Items and weapons via numbers
        for (int i = 0; i < items.Capacity; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        //ScrollWheel change weapon
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Capacity - 1)
                EquipItem(0);
            else
                EquipItem(itemIndex + 1);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
                EquipItem(items.Capacity - 1);
            else
                EquipItem(itemIndex - 1);
        }

        //delete the Weapon (grenades) if they are all used from list
        if (items[itemIndex].GetComponent<ThrowGun>() != null)
        {
            if (items[itemIndex].GetComponent<ThrowGun>().GetCurrentAmmo() == 0 && items[itemIndex].GetComponent<ThrowGun>().GetTotalAmmo() == 0)
            {
                EquipItem(0);
                items.RemoveAt(items.Capacity - 1);
            }
        }
    }

    void Jump()
    {
        //Jump

        jumpSound.Play(transform, body);
        velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);

        //Stamina lost
        //currentStamina -= jumpStaminaCost;
        //CheckStamina();
        //Debug.Log("Salto y me queda " + currentStamina + " de estamina");

    }

    void Leap()
    {
        //Leap if Hidden

        jumpSound.Play(transform, body);
        velocity.x = cam.forward.x * leapForce;
        velocity.z = cam.transform.forward.z * leapForce;
        velocity.y = cam.transform.forward.y * leapForce;

        //Stamina lost
        currentStamina -= jumpStaminaCost;
        CheckStamina();
        //Debug.Log("Leap");

        //After launching we reset the speed
        StartCoroutine(ResetVelocity());
    }

    /*
    void Interact(bool interact)
    {

        RaycastHit hit;

        if (interact)
        {

            if (!IsInteracting)
            {

                IsInteracting = true;
                Debug.DrawRay(cam.position, cam.forward, Color.blue, 3.0f);
                if (Physics.Raycast(cam.position, cam.forward, out hit, 1.0f))
                {
                    Debug.Log("I interacted with " + hit.transform.name);
                    objectInteracted = hit.transform;
                    objectInteracted.transform.parent = transform;
                    objectInteracted.transform.GetComponent<Rigidbody>().isKinematic = !enabled;
                }

            }
        }
        else
        {
            IsInteracting = false;
            if (objectInteracted != null)
            {

                objectInteracted.transform.parent = null;
                if (objectInteracted.GetComponent<Rigidbody>() != null)
                    objectInteracted.GetComponent<Rigidbody>().isKinematic = enabled;
            }
            else
                return;
        }
    }
    */

    IEnumerator ResetVelocity()
    {
        yield return new WaitForSeconds(0.7f);
        velocity = Vector3.zero;
        //Debug.Log("Reseteamos velocidad!");
    }

    void Move()
    {
        //If we artifically leave the player without movement
        if (canMove)
        {

            //Check if player is grounded
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2.0f;
            }

            //Input
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL, x);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL, z);

            if (Input.GetAxis("Vertical") >= 0.01f || Input.GetAxis("Horizontal") >= 0.01f || Input.GetAxis("Vertical") <= -0.01f || Input.GetAxis("Horizontal") <= -0.01f)
            {
                //Debug.Log ("Player is moving");
                playerIsMoving = true;
                firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 1f);
            }
            else if (Input.GetAxis("Vertical") == 0 || Input.GetAxis("Horizontal") == 0)
            {
                //Debug.Log ("Player is not moving");
                playerIsMoving = false;
                firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 0f);
            }

            //Basic Movement
            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * currentSpeed * Time.deltaTime);
        }

    }

    void CallFootsteps()
    {
        if (playerIsMoving == true)
        {
            //Debug.Log("Step Sound");
            //Debug.Log ("Player is moving");
            walkSound.Play(transform);
        }
    }

    void OnDisable()
    {
        playerIsMoving = false;
    }

    public bool GetplayerIsMoving()
    {
        return playerIsMoving;
    }

    void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;
        items[itemIndex].itemGameObject.SetActive(true);
        firstPersonAnimator = items[itemIndex].itemGameObject.GetComponent<Animator>();

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);

        }
        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;

        healthbarImage.fillAmount = currentHealth / maxHealth;

        hurtSound.Play(transform);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        //Activate ragdoll and detach it from the player
        PV.RPC("RPC_Die", RpcTarget.All);

        //Die on server
        playerManager.Die();
    }

    [PunRPC]
    void RPC_Die()
    {
        deadSound.Play(transform);

        //Activate Ragdoll and unparent to all server
        userBody.SetActive(true);
        userBody.gameObject.SetActive(true);
        userBody.transform.parent = null;
        userBody.name = "Ragdoll";

        userBody.GetComponentInChildren<Ragdoll>().SetEnabled(true);

    }

    void RegenStamina()
    {
        if (Time.time - lastRegen > staminaRegenSpeed && !isSprinting && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenAmmount;
            lastRegen = Time.time;
            CheckStamina();
        }
    }

    void CheckStamina()
    {
        if (currentStamina >= maxStamina)
            currentStamina = maxStamina;

        if (currentStamina <= 0f)
            currentStamina = 0f;
        staminabarImage.fillAmount = currentStamina / maxStamina;
        canMove = true;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!isGrounded && hit.normal.y < 0.3f)
        {
            if (Input.GetButton("Jump"))
            {
                //Able to wall jump
                Debug.DrawRay(hit.point, hit.normal, Color.red);
                Jump();
                Debug.Log("Jumping off a wall!");
            }
            if (Input.GetButtonDown("Leap"))
            {
                Leap();
            }
            /*
            if (Input.GetButtonDown("Aim") && currentStamina >= sprintStaminaCost)
            {
                //No gravity so the player does not fall
                gravity = 0;
                canMove = false;
                //Lose stamina while hanging
                currentStamina -= sprintStaminaCost * Time.deltaTime;
                CheckStamina();

                Debug.Log("Am I stuck into the wall?");
            }
            else if (Input.GetButtonUp("Aim"))
            {
                //Restore gravity
                gravity = -9.81f;
                canMove = true;
            }
            */
        }


    }

    public void DecreaseSpeed(float ammount)
    {
        speed -= ammount;
        sprintSpeed -= ammount;
    }

    public void IncreaseSpeed(float ammount)
    {
        speed += ammount;
        sprintSpeed += ammount;
    }
}
