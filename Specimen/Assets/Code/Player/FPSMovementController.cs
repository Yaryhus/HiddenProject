using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using EZCameraShake;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Rigidbody))]
public class FPSMovementController : MonoBehaviourPunCallbacks
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


    //Private variables
    Vector3 velocity;
    bool isGrounded;
    CharacterController controller;
    Rigidbody body;
    bool playerIsMoving = false;

    PhotonView PV;

    void Start()
    {

        if (PV.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(body);
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

        jumpSound.Init();
        walkSound.Init();

        InvokeRepeating("CallFootsteps", 0, timeBetweenSteps);
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine)
            return;
        //Check if player is grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2.0f;
        }

        //Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

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

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }
        previousItemIndex = itemIndex;

        if(PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

}
