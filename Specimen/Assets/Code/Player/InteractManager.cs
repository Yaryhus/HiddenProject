using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager : MonoBehaviour
{

    [SerializeField] float intereactableRange = 5.0f;
    [SerializeField] Transform holdParent;
    [SerializeField] float moveForce = 250;
    [SerializeField] GameObject ropeObject;
    public bool isHolding = false;

    private GameObject heldObj;
    private FPSMovementController parentController;

    // Start is called before the first frame update
    void Start()
    {
        parentController = GetComponent<FPSMovementController>();
        //holdParent = transform;
    }

    // Update is called once per frame
    void Update()
    {


        //If we holding an object, we move it with us
        if (heldObj != null)
        {
            MoveObject();
        }
    }

    void MoveObject()
    {
        if (Vector3.Distance(heldObj.transform.position, holdParent.position) > 0.1f)
        {
            Vector3 MoveDirection = (holdParent.position - heldObj.transform.position);
            heldObj.GetComponent<Rigidbody>().AddForce(moveForce * MoveDirection);
        }
    }

    public void PickUpObject()
    {
        if (heldObj == null)
        {
            isHolding = true;
            RaycastHit hit;
            //Get object, save it and parent it to us.
            Debug.DrawRay(parentController.cam.position, parentController.cam.forward, Color.cyan, 5.0f);
            if (Physics.Raycast(parentController.cam.position, parentController.cam.forward, out hit, intereactableRange))
            {
                if (hit.transform.gameObject.GetComponent<Rigidbody>())
                {
                    Rigidbody objRig = hit.transform.gameObject.GetComponent<Rigidbody>();
                    objRig.useGravity = false;
                    objRig.drag = 10;

                    objRig.transform.parent = holdParent;
                    heldObj = hit.transform.gameObject;
                }
            }
        }
        else
            DropObject();
    }

    public void DropObject()
    {
        Rigidbody heldRig = heldObj.GetComponent<Rigidbody>();
        heldRig.useGravity = true;
        heldRig.drag = 1;

        heldObj.transform.parent = null;
        heldObj = null;
        isHolding = false;
    }

    public void AttachToObject()
    {
        GameObject ropeInstance;
        //Generate a rope
        ropeInstance = Instantiate(ropeObject, Vector3.zero,Quaternion.identity);
        ropeInstance.transform.parent = heldObj.transform;

        Rope rope = ropeInstance.GetComponent<Rope>();
        //Assign origin to this and end point to whatever is in reach and can be used.
        rope.origin = heldObj.transform;

        RaycastHit hit;
        if (Physics.Raycast(heldObj.transform.position, parentController.cam.forward, out hit, intereactableRange))
        {
            rope.endObject = hit.transform;
        }
        //Start grapple
        rope.StartGrapple();
        DropObject();
    }
}
