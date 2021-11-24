using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    GameObject controller;
    [SerializeField]
    GameObject canvas;
    [SerializeField]
    GameObject readyButton;
    GameObject cam;

    GameObject primaryWeaponsGO;
    GameObject secondaryWeaponsGO;
    GameObject gadgetWeaponsGO;

    int mainWep = -1, seconWep = -1, gadgetWep = -1;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        cam = GetComponentInChildren<Camera>().gameObject;
    }

    void Start()
    {
        cam = GetComponentInChildren<Camera>().gameObject;

        primaryWeaponsGO = GameObject.Find("Primary Weapons");
        secondaryWeaponsGO = GameObject.Find("Secondary Weapons");
        gadgetWeaponsGO = GameObject.Find("Gadgets");

        /*
        if (PV.IsMine)
        {
            CreateController();
        }*/

    }

    private void Update()
    {
        if (mainWep != -1 && seconWep != -1 && gadgetWep != -1)
        {
            readyButton.SetActive(true);
        }
        else
            readyButton.SetActive(false);
    }

    void CreateController()
    {
        canvas.SetActive(false);
        cam.SetActive(false);

        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", GlobalVariablesAndStrings.PLAYERNAME_POLICE), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
        controller.GetComponent<FPSMovementController>().ChangeWeapon(mainWep, seconWep, gadgetWep);
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();
    }

    public void StartButton()
    {
        if (PV.IsMine)
        {
            CreateController();
        }
    }

    public void ChangeWeaponRifle()
    {
        mainWep = GlobalVariablesAndStrings.WEAPON_RIFLE;
        primaryWeaponsGO.SetActive(false);

        Debug.Log("Chose Rifle");
    }
    public void ChangeWeaponShotgun()
    {
        mainWep = GlobalVariablesAndStrings.WEAPON_SHOTGUN;
        primaryWeaponsGO.SetActive(false);

        Debug.Log("Chose Shotgun");
    }
    public void ChangeWeaponPistol()
    {
        seconWep = GlobalVariablesAndStrings.WEAPON_PISTOL;
        secondaryWeaponsGO.SetActive(false);

        Debug.Log("Chose Pistol");
    }
    public void ChangeWeaponSonicAlarm()
    {
        gadgetWep = GlobalVariablesAndStrings.WEAPON_SONICALARM;
        gadgetWeaponsGO.SetActive(false);

        Debug.Log("Chose Sonic Alarm");
    }

    public void Reset()
    {
        primaryWeaponsGO.SetActive(true);
        secondaryWeaponsGO.SetActive(true);
        gadgetWeaponsGO.SetActive(true);
        seconWep = -1;
        gadgetWep = -1;

    }

}
