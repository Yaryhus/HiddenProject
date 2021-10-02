using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{

    [SerializeField] private Animator animator;

    private Rigidbody[] rigidbodies;

    // Start is called before the first frame update
    void Start()
    {
        rigidbodies = transform.GetComponentsInChildren<Rigidbody>();
        Debug.Log(rigidbodies.Length);
        SetEnabled(false);
    }

    public void SetEnabled(bool enabled)
    {
        Debug.Log("I have been called");
        bool isKinematic = !enabled;
        foreach (Rigidbody r in rigidbodies)
        {
            r.isKinematic = isKinematic;
        }

        animator.enabled = !enabled;
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetEnabled(true);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            SetEnabled(false);
        }
        
    }

}
