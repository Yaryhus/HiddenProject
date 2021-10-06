using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{

    ObjectPooler objectPooler;

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        objectPooler.SpawnFromPool("RifleBullets", transform.position, transform.rotation);
    }
}
