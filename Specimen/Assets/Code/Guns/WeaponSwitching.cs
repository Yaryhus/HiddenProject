using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField]
    int selectedWeapon = 0;
    void Start()
    {

    }
    void Update()
    {
        int previousSelectedWeapon = selectedWeapon;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            else
                selectedWeapon--;
        }

        //When I learn how to do a switch case of Input.GetkeyDowns, I will be back
        if (Input.GetKeyDown(KeyCode.Alpha1))
            selectedWeapon = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
            selectedWeapon = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
            selectedWeapon = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
            selectedWeapon = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5) && transform.childCount >= 5)
            selectedWeapon = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6) && transform.childCount >= 6)
            selectedWeapon = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7) && transform.childCount >= 7)
            selectedWeapon = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha8) && transform.childCount >= 8)
            selectedWeapon = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha9) && transform.childCount >= 9)
            selectedWeapon = 8;

        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);

            i++;
        }
    }
}
