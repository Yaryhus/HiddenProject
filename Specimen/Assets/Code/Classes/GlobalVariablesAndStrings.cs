﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariablesAndStrings : MonoBehaviour
{
    //---------------- MENU STRINGS ------------------------
    public static  string MENU_NAME_LOADING = "loading";
    public static string MENU_NAME_MAINMENU = "title";
    public static string MENU_NAME_CREATEROOM = "create room";
    public static string MENU_NAME_ROOM = "room";
    public static string MENU_NAME_ERRORMENU = "error";
    public static string MENU_NAME_FINDROOMMENU = "find room";

    //---------------- PLAYER ------------------------------
    public static Transform PLAYER;

    //------------ MAPS IDs in Build Settings --------------
    public static int  MAP_MAINMENU = 0;
    public static int MAP_FPSTESTMAP = 1;

    //----------- ANIMATIONS NAMES AND VARIABLES----------
    // Third Person animation variables
    public static string ANIM3_TRIGGER_SHOOTSEMI = "Shoot Semi";
    public static string ANIM3_TRIGGER_SPECIAL_ATTACK = "Special Attack";
    public static string ANIM3_TRIGGER_JUMP = "Jump";
    public static string ANIM3_BOOLEAN_CROUCHED = "Crouched";
    public static string ANIM3_BOOLEAN_SHOOTAUTO = "Shoot Auto";
    public static string ANIM3_BOOLEAN_INAIR = "InAir";
    public static string ANIM3_FLOAT_VERTICAL = "Vertical";
    public static string ANIM3_FLOAT_HORIZONTAL = "Horizontal";
    // First Person animation variables
    public static string ANIM1_TRIGGER_SHOOT = "Shoot";
    public static string ANIM1_SPECIALATTACK = "Special Attack";
    public static string ANIM1_TRIGGER_RELOAD = "Reload";
    public static string ANIM1_TRIGGER_SHOWWEAPON = "ShowWeapon";
    public static string ANIM1_TRIGGER_HIDEWEAPON = "HideWeapon";
    public static string ANIM1_BOOLEAN_SHOOTING = "Shooting";
    public static string ANIM1_BOOLEAN_AIM = "Aim";
    public static string ANIM1_FLOAT_WALK = "Walk";
    public static string ANIM1_TAUNT = "Taunt";


}
