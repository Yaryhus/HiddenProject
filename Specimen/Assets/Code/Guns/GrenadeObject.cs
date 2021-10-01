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
        foreach(Collider col in colliders)
        {
            Rigidbody rig = col.GetComponent<Rigidbody>();
            //we collide with something that is not a Player
            if (rig != null && !col.transform.tag.Equals("Player"))
            {               
                rig.AddExplosionForce(explosionForce, transform.position, radius, 1f, ForceMode.Impulse);
            }
            col.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);
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
