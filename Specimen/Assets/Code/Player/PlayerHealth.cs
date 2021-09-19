using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField]
    float health = 100;

    bool isDead = false;
    PlayerManager playerManager;

    [Header("Sounds")]
    [SerializeField]
    Sound hurtSound = null;
    [SerializeField]
    Sound deadSound = null;

    private void Awake()
    {
        //playerManager = PhotonView.Find(PV.InstantiationData[0]).GetComponent<PlayerManager>();
    
    }
    public void TakeDamage(float amount)
    {
        if (!isDead)
        {
            hurtSound.Play(transform);
            health -= amount;
            if (health <= 0)
                Die();
        }
    }

    void Die()
    {
        deadSound.Play(transform);
        //is dead, so no more "takeDamage"
        isDead = true;
        //Dead code here
        Debug.Log("Hay man matao");
        //Destroy(gameObject, 15.0f);
    }
}
