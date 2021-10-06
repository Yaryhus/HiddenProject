using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSleeve : MonoBehaviour, IPoolObject
{

    public float upForce = 2.0f;
    public float sideForce = .5f;
    private WaitForSeconds wait;
    private float waitTime = 10.0f;

    public void Start()
    {
        wait = new WaitForSeconds(waitTime);
    }

    public void OnObjectSpawn()
    {

        float xForce = Random.Range(sideForce / 2, sideForce);
        float yForce = Random.Range(upForce / 2, upForce);
       float zForce = Random.Range(-sideForce / 2, sideForce / 2);

        Vector3 force = new Vector3(0, yForce, xForce);

        GetComponent<Rigidbody>().velocity = force;

        //Disable after time
        StartCoroutine(Disable());
    }

    private IEnumerator Disable()
    {
        yield return wait;
        gameObject.SetActive(false);

    }
}
