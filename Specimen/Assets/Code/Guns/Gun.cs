using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Item
{
    public abstract override void Use();
    public abstract void Reload();
    public abstract void Aim();
    public abstract void UpdateText();

    //Animator events
    public abstract void AttackEnd();
    public abstract void AttackStart();
    public abstract void DetectDamage();

    //Change weapons
    public abstract void OnChangeWeapon();

    public GameObject bulletImpactPrefab;
    public GameObject parent;
}
