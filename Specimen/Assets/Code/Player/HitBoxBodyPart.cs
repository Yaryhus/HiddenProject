using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxBodyPart : MonoBehaviour
{
    //Drop bullets from a pool of bullets from certain prefab
    [ValueDropdown("BodyPart")]
    public string bodypart;
    string[] BodyPart = { "ArmsOrLegs", "Torso", "Head" };

    public string GetBodyPartType()
    {
        return bodypart;
    }

}
