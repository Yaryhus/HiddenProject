using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponIK : MonoBehaviourPunCallbacks
{

    [SerializeField] Transform targetTransform;
    [SerializeField] Transform aimTransform;
    [SerializeField] Transform bone;
    PhotonView PV;
    //Vector3 targetPosition = Vector3.zero;

    int iterations = 10;


    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!PV.IsMine)
            return;

        if (targetTransform != null)
        {

            Vector3 targetPosition = targetTransform.position;
            for (int i = 0; i < iterations; i++)
            {
                AimAtTarget(bone, targetPosition);
            }
        }
    }

    private void AimAtTarget(Transform bone, Vector3 targetPosition)
    {
        Vector3 aimDirection = aimTransform.forward;
        Vector3 targetDirection = targetPosition - aimTransform.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        bone.rotation = aimTowards * bone.rotation;
        //PV.RPC("RPC_AimAtTarget", RpcTarget.All, bone, targetPosition);
    }
    /*
    [PunRPC]
    void RPC_AimAtTarget(Transform bone)
    {
        Vector3 aimDirection = aimTransform.forward;
        Vector3 targetDirection = targetPosition - aimTransform.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        bone.rotation = aimTowards * bone.rotation;
    }
    */
}
