using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{

    [SerializeField] private Animator animator;

    //Times a body can be utilized for food
    [SerializeField] public float healUses = 3;

    private Rigidbody[] rigidbodies;

    // Start is called before the first frame update
    void Start()
    {
        rigidbodies = transform.GetComponentsInChildren<Rigidbody>();
        //Debug.Log(rigidbodies.Length);
        //SetEnabled(false);
    }

    public void SetEnabled(bool enabled)
    {
        //Debug.Log("I have been called");
        bool isKinematic = !enabled;

        if (rigidbodies != null)
        {

            foreach (Rigidbody r in rigidbodies)
            {
                if (r != null)
                    r.isKinematic = isKinematic;
            }
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

    public void DecreaseHealUses(bool hasHiddenHealed)
    {
        if (hasHiddenHealed)
            healUses--;
    }
}
