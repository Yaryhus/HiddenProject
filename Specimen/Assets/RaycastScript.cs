using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastScript : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + transform.forward * 50, Color.blue);
        //Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, transform.position + transform.forward * 50);
    }
}
