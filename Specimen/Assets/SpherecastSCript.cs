using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherecastSCript : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
