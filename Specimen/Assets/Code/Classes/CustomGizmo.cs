using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGizmo : MonoBehaviour
{
    [SerializeField]
    Color color = Color.blue;
    [SerializeField]
    float size = 1;
    Vector3 vectorSize;
    // Start is called before the first frame update
    void Start()
    {
         vectorSize = new Vector3(size, size, size);
    }
        void OnDrawGizmos()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}
