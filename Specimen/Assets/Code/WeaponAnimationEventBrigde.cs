using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimationEventBrigde : MonoBehaviour
{
    GameObject parent;

    private void Start()
    {
        parent = transform.parent.gameObject;
    }


    //These are intermediary methods to pass an animation event from this gameobject to the parent that does the actual code. As the script called must be in the same gameobject as the animations
    public void AttackStart()
    {
        parent.GetComponent<Gun>().AttackStart();
    }

    public void AttackEnd()
    {
        parent.GetComponent<Gun>().AttackEnd();
    }

    public void DetectDamage()
    {
        parent.GetComponent<Gun>().DetectDamage();
    }
}
