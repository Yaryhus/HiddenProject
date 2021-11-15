using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSight : MonoBehaviour
{
    Camera cam;
    [SerializeField]
    LayerMask ignoreLayers;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        //Debug.DrawRay(ray.origin, cam.transform.forward*1000f, Color.red, 3.0f);
        if (Physics.Raycast(ray.origin, cam.transform.forward, out RaycastHit hit, 1000f, ~ignoreLayers))
        {
            transform.position = hit.point + hit.normal * 0.002f;
            transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
        }

    }
}
