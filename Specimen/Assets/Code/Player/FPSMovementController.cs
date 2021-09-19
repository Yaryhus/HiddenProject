using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using EZCameraShake;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Rigidbody))]
public class FPSMovementController : MonoBehaviour
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
    Sound walkSound  = null;
    [SerializeField]
    float timeBetweenSteps = 0.2f;
    [SerializeField]
    Sound jumpSound  = null;

    //Private variables
    Vector3 velocity;
    bool isGrounded;
    CharacterController controller;
    Rigidbody body;
    bool playerIsMoving = false;

    PhotonView PV;

    void Start()
    {

        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<CameraShaker>().gameObject);
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
}
