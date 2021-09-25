using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using EZCameraShake;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Rigidbody))]
public class FPSMovementController : MonoBehaviourPunCallbacks, IDamageable
{
    //Public Variables
    [Header("Main Movement")]
    [SerializeField]
    float speed = 12.0f;

    [Header("Jump")]
    [SerializeField]
    float jumpHeight = 3.0f;

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

    [Header("Weapons and Items")]
    [SerializeField]
    Item[] items;

    int itemIndex;
    int previousItemIndex = -1;

    [Header("UI")]
    [SerializeField]
    GameObject ui;

    [Header("Health Stats")]
    [SerializeField]
    float maxHealth = 100f;

    float currentHealth;
    [SerializeField]
    Image healthbarImage;

    bool isDead = false;
    PlayerManager playerManager;

    [Header("Health Sounds")]
    [SerializeField]
    Sound hurtSound = null;
    [SerializeField]
    Sound deadSound = null;

    [Header("Animations")]
    [SerializeField]
    Animator thirdPersonAnimator;
    Animator firstPersonAnimator;

    [Header("Body for third person")]
    [Tooltip("Temporal: With this apporach local user does not see themself but there are no shadows or legs to look locally")]
    [SerializeField]
    GameObject userBody;
    
    //Private variables
    Vector3 velocity;
    bool isGrounded;
    CharacterController controller;
    Rigidbody body;
    bool playerIsMoving = false;

    PhotonView PV;

    void Start()
    {
        currentHealth = maxHealth;
        if (PV.IsMine)
        {
            EquipItem(0);
            //For now, deactivate 3rd person view (Will have to change this later so the user can see their feet or have shadows)
            userBody.SetActive(false);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(body);
            Destroy(ui);
        }

        controller = GetComponent<CharacterController>();
        PV = GetComponent<PhotonView>();
        body = GetComponent<Rigidbody>();
        GlobalVariablesAndStrings.PLAYER = transform;

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

        //Jump
        if (Input.GetButton("Jump") && isGrounded)
        {
            jumpSound.Play(transform, body);
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        //Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


        //Switching Items and weapons via numbers
        for (int i = 0; i < items.Length; i++)
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
            if (itemIndex >= items.Length - 1)
                EquipItem(0);
            else
                EquipItem(itemIndex + 1);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
                EquipItem(items.Length - 1);
            else
                EquipItem(itemIndex - 1);
        }

        //Use item
        if (Input.GetButtonDown("Shoot"))
        {
            items[itemIndex].Use();
        }

        //Reload item (weapon)
        if (Input.GetButtonDown("Reload"))
        {
            //If it is a gun, we reload it
            ((Gun)items[itemIndex]).Reload();
        }

        //falling off the map
        if (transform.position.y < -100.0f)
        {
            Die();
        }


        //update text for ammo if the item is a SingleShotGun
        ((SingleShotGun)items[itemIndex]).UpdateText();

    }

    void Move()
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
        }
        else if (Input.GetAxis("Vertical") == 0 || Input.GetAxis("Horizontal") == 0)
        {
            //Debug.Log ("Player is not moving");
            playerIsMoving = false;
        }

        //Basic Movement
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

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
            //Start hide weapon anim
            //firstPersonAnimator.SetTrigger(GlobalVariablesAndStrings.ANIM1_TRIGGER_HIDEWEAPON);
            /*
            //Wait until finished and deactivate object
            while(firstPersonAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            {
                Debug.Log("Oh no");
            }
            */
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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }

}
