using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField]
    float health = 100;
    bool isDead = false;

    [Header("Sounds")]
    [SerializeField]
    Sound hurtSound = null;
    [SerializeField]
    Sound deadSound = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
