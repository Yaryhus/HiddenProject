using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeObject : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] GameObject explosionEffect;
    public float delay = 1f;
    public float explosionForce = 1f;
    public float radius = 1f;
    public float damage = 1f;

    [Header("Sounds")]
    [SerializeField]
    Sound explosionSound = null;

    PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Explode", delay);
    }

    // Update is called once per frame
    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider col in colliders)
        {
            Rigidbody rig = col.GetComponent<Rigidbody>();
            //we collide with something that is not a Player
            if (rig != null && !col.transform.tag.Equals("Player"))
            {
                rig.AddExplosionForce(explosionForce, transform.position, radius, 1f, ForceMode.Impulse);
            }

            //Distance between grenade and user
            float distance = Vector3.Distance(transform.position, col.transform.position);
            distance = Mathf.Clamp01(1 - (distance / radius));
            if (col.transform.tag.Equals("Player"))
            {

                Debug.Log("The grenade did " + damage * distance);
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, (col.transform.position - transform.position), out hit))
            {
                //if grenade is in Line of Sight of the collision
                if (hit.transform.gameObject == col.gameObject)
                    col.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage * distance);
            }
        }
        explosionSound.PlayOneShot(transform);
        Instantiate(explosionEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
