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
    public bool isHidden = false;
    [ShowIf("isHidden")]
    [SerializeField]
    GameObject ThermalCameraGO;

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
    [SerializeField]
    float hiddenHealAmmount = 15.0f;

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
    [SerializeField]
    List<GameObject> thirdPersonItems;

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
    GameObject firstPersonWeapons;
    [SerializeField]
    GameObject thirdPersonWeapons;

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

    bool usingThermalVision = false;

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
        //thirdPersonWeapons = transform.Find("WeaponHolder").gameObject;



        //Find what body are we using to get the animator
        thirdPersonAnimator = userBody.GetComponentInChildren<Animator>();



        if (PV.IsMine)
        {
            EquipItem(0);
            //For now, deactivate 3rd person view (Will have to change this later so the user can see their feet or have shadows). We use a 
            Transform[] children = userBody.GetComponentsInChildren<Transform>(true);
            foreach (Transform g in children)
            {
                g.gameObject.layer = GlobalVariablesAndStrings.LAYER_PLAYERCAMERAIGNORE;
            }
            transform.gameObject.layer = GlobalVariablesAndStrings.LAYER_PLAYERCAMERAIGNORE;
        }
        else
        {
            Destroy(cam.parent.gameObject);
            Destroy(body);
            Destroy(ui);
            controller.enabled = false;
            firstPersonWeapons.SetActive(false);
            //userBody.SetActive(true);
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
        thirdPersonAnimator = userBody.GetComponentInChildren<Animator>();


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
            //Debug.Log("Holding the mouse button down");
            ShootAuto();
        }
        else if (Input.GetButtonDown("Shoot") && interactManager.isHolding.Equals(false))
        {
            //Debug.Log("Clicking the mouse button ");
            ShootSemi();
        }

        //if (items[itemIndex].GetComponent<SingleShotGun>() != null && items[itemIndex].GetComponent<SingleShotGun>().GetIsAutomatic() && Input.GetButtonUp("Shoot") && interactManager.isHolding.Equals(false))
        //thirdPersonAnimator.SetLayerWeight(itemIndex + 1, 0.0f);


        /*
        //Shooting while carrying an object attachs it to the wall
        if (Input.GetButtonDown("Shoot") && interactManager.isHolding.Equals(true))
        {
            interactManager.AttachToObject();
        }
        */

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

        if (Input.GetButtonDown("ThermalVision") && ThermalCameraGO != null)
        {
            if (usingThermalVision)
            {
                usingThermalVision = false;
                ThermalCameraGO.SetActive(false);
            }
            else
            {
                usingThermalVision = true;
                ThermalCameraGO.SetActive(true);

            }
        }

    }

    private void UpdateAmmoText()
    {
        ((Gun)items[itemIndex]).UpdateText();
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
        ChangeAnimationTrigger(GlobalVariablesAndStrings.ANIM3_TRIGGER_SPECIAL_ATTACK);
    }

    private void ShootSemi()
    {
        //Debug.Log("ShootSemi");
        items[itemIndex].Use();
        ChangeAnimationTrigger(GlobalVariablesAndStrings.ANIM3_TRIGGER_SHOOTSEMI);

    }

    private void ShootAuto()
    {
        //Particular case: Automatic fire
        if (Time.time - nextTimeToFire > 1 / items[itemIndex].GetComponent<SingleShotGun>().fireRate)
        {
            //Debug.Log("Shooting Auto");
            nextTimeToFire = Time.time;
            items[itemIndex].Use();
            ChangeAnimationTrigger(GlobalVariablesAndStrings.ANIM3_TRIGGER_SHOOTSEMI);
        }
    }

    void Sprint()
    {
        //Sprint
        if (Input.GetButton("Sprint") && playerIsMoving && currentStamina >= sprintStaminaCost)
        {
            currentSpeed = sprintSpeed;
            currentStamina -= sprintStaminaCost * Time.deltaTime * 10f;
            isSprinting = true;
            CheckStamina();

            firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 2f);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL, thirdPersonAnimator.GetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL) + 1.0f);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL, thirdPersonAnimator.GetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL) + 1.0f);

        }
        //Run out of sprint but I am still pressing the sprint button, or any other action
        else if (Input.GetButton("Sprint") && playerIsMoving && currentStamina < sprintStaminaCost)
        {
            currentSpeed = speed;
            isSprinting = false;
            CheckStamina();

            firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 1f);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL, thirdPersonAnimator.GetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL) - 1.0f);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL, thirdPersonAnimator.GetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL) - 1.0f);
        }
        /*
        else
        {
            currentSpeed = speed;
            isSprinting = false;
            CheckStamina();

            firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 0f);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL, thirdPersonAnimator.GetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL) - 1.0f);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL, thirdPersonAnimator.GetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL) - 1.0f);
        }
        */

        if (Input.GetButtonUp("Sprint"))
        {
            currentSpeed = speed;
            isSprinting = false;
            CheckStamina();

            firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 1f);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL, thirdPersonAnimator.GetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL) - 1.0f);
            thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL, thirdPersonAnimator.GetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL) - 1.0f);
        }
    }

    void Crouch()
    {
        //Crouch
        if (Input.GetButtonDown("Crouch"))
        {
            //transform.position = new Vector3(transform.position.x, transform.position.y - 0.9f, transform.position.z);
            controller.height = 1.0f;
            userBody.transform.position = new Vector3(userBody.transform.position.x, userBody.transform.position.y + 0.9f, userBody.transform.position.z);
            controller.center = new Vector3(controller.center.x, 0.2f, controller.center.z);

            thirdPersonAnimator.SetBool(GlobalVariablesAndStrings.ANIM3_BOOLEAN_CROUCHED, true);
        }
        if (Input.GetButtonUp("Crouch"))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.9f, transform.position.z);
            controller.height = 1.9f;
            controller.center = new Vector3(controller.center.x, -0.1f, controller.center.z);
            userBody.transform.position = new Vector3(userBody.transform.position.x, userBody.transform.position.y - 0.9f, userBody.transform.position.z);

            thirdPersonAnimator.SetBool(GlobalVariablesAndStrings.ANIM3_BOOLEAN_CROUCHED, false);

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
            {
                EquipItem(0);
            }
            else
            {
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                EquipItem(items.Capacity - 1);
            }
            else
            {
                EquipItem(itemIndex - 1);
            }
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
        ChangeAnimationTrigger(GlobalVariablesAndStrings.ANIM3_TRIGGER_JUMP);
        jumpSound.Play(transform, body);

        velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);

        thirdPersonAnimator.SetBool(GlobalVariablesAndStrings.ANIM3_BOOLEAN_INAIR, true);
    }

    //Leap if Hidden
    void Leap()
    {
        jumpSound.Play(transform, body);

        velocity.x = cam.forward.x * leapForce;
        velocity.z = cam.forward.z * leapForce;

        velocity.y = cam.forward.y * leapForce;
        ChangeAnimationTrigger(GlobalVariablesAndStrings.ANIM3_TRIGGER_JUMP);
        thirdPersonAnimator.SetBool(GlobalVariablesAndStrings.ANIM3_BOOLEAN_INAIR, true);

        //Stamina lost
        currentStamina -= jumpStaminaCost;
        CheckStamina();

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
        yield return new WaitForSeconds(0.3f);
        velocity = Vector3.zero;
        thirdPersonAnimator.SetBool(GlobalVariablesAndStrings.ANIM3_BOOLEAN_INAIR, false);
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
                thirdPersonAnimator.SetBool(GlobalVariablesAndStrings.ANIM3_BOOLEAN_INAIR, false);
                velocity.y = -2.0f;
            }

            //Input
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");


            if (Input.GetAxis("Vertical") >= 0.01f || Input.GetAxis("Horizontal") >= 0.01f || Input.GetAxis("Vertical") <= -0.01f || Input.GetAxis("Horizontal") <= -0.01f)
            {
                //Debug.Log ("Player is moving");
                playerIsMoving = true;
                firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 1f);
                thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL, x);
                thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL, z);
            }
            else if (Input.GetAxis("Vertical") == 0 || Input.GetAxis("Horizontal") == 0)
            {
                //Debug.Log ("Player is not moving");
                playerIsMoving = false;
                firstPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM1_FLOAT_WALK, 0f);
                thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_HORIZONTAL, 0);
                thirdPersonAnimator.SetFloat(GlobalVariablesAndStrings.ANIM3_FLOAT_VERTICAL, 0);
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
        if (items[itemIndex].itemGameObject != null)
            items[itemIndex].itemGameObject.SetActive(true);
        if (thirdPersonItems[itemIndex] != null)
            thirdPersonItems[itemIndex].SetActive(true);
        //Debug.Log(itemIndex);

        firstPersonAnimator = items[itemIndex].itemGameObject.GetComponent<Animator>();
        thirdPersonAnimator.SetLayerWeight(itemIndex + 1, 1.0f);

        if (previousItemIndex != -1)
        {
            thirdPersonAnimator.SetLayerWeight(previousItemIndex + 1, 0.0f);

            ((Gun)items[previousItemIndex]).OnChangeWeapon();
            items[previousItemIndex].itemGameObject.SetActive(false);
            thirdPersonItems[previousItemIndex].SetActive(false);

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

    public bool HiddenHeal()
    {
        //We heal, unless we have max heal in which case we wont heal.
        if (currentHealth == maxHealth)
        {
            healthbarImage.fillAmount = currentHealth / maxHealth;
            return false;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth + hiddenHealAmmount, maxHealth);
            healthbarImage.fillAmount = currentHealth / maxHealth;
            return true;
        }
    }

    void ChangeAnimationTrigger(string name)
    {
        PV.RPC("RPC_ChangeAnimationTrigger", RpcTarget.All, name);

    }

    [PunRPC]
    void RPC_ChangeAnimationTrigger(string triggerName)
    {
        thirdPersonAnimator.SetTrigger(triggerName);
    }

    void ChangeAnimationBool(string name, bool state)
    {
        string[] parameters = { name, state.ToString() };
        PV.RPC("RPC_ChangeAnimationTrigger", RpcTarget.All, parameters);

    }

    [PunRPC]
    void RPC_ChangeAnimationBool(string triggerName, bool bo)
    {
        thirdPersonAnimator.SetBool(triggerName, bo);
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
        // userBody.SetActive(true);
        //userBody.gameObject.SetActive(true);
        //userBody.transform.parent = null;

        GameObject ragdollChild = userBody.GetComponentInChildren<Ragdoll>().gameObject;
        ragdollChild.SetActive(true);
        ragdollChild.gameObject.SetActive(true);
        ragdollChild.transform.parent = null;

        //Name and Ragdoll tag
        ragdollChild.name = "Ragdoll";
        ragdollChild.layer = 9;
        Transform[] allChildren = ragdollChild.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            //First we hide the "Thermal version" of the model
            if (child.gameObject.layer == 11)
                child.gameObject.SetActive(false);
            //We give everything the ragdoll layer
            child.gameObject.layer = 9;
        }

        ragdollChild.GetComponentInChildren<Ragdoll>().SetEnabled(true);
        //userBody.GetComponentInChildren<Ragdoll>().SetEnabled(true);

    }

    void RegenStamina()
    {
        if (Time.time - lastRegen > staminaRegenSpeed && !isSprinting && currentStamina < maxStamina && !Input.GetButton("Jump") && !Input.GetButton("Sprint"))
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
        if (!isGrounded && hit.normal.y < 0.3f && isHidden)
        {
            if (Input.GetButton("Jump") && currentStamina >= jumpStaminaCost)
            {
                //Able to wall jump
                Debug.DrawRay(hit.point, hit.normal, Color.red);
                Jump();

                //Stamina lost, as this jump does take stamina
                currentStamina -= jumpStaminaCost;
                CheckStamina();

                //Debug.Log("Jumping off a wall!");
            }
            if (Input.GetButtonDown("Leap") && currentStamina >= jumpStaminaCost)
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
