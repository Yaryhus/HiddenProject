using System.Collections;
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
    public static float DAMAGE_MULT_ARMORLEG = 0.5f;
    public static float DAMAGE_MULT_TORSO = 1f;
    public static float DAMAGE_MULT_HEAD = 2.0f;
    public static string PLAYERNAME_HIDDEN = "HiddenController";
    public static string PLAYERNAME_POLICE = "PoliceController";

    //---------------- PLAYER ------------------------------
    public static int LAYER_DEFAULT = 0;
    public static int LAYER_TRANSPARENTFX = 1;
    public static int LAYER_IGNORERAYCAST = 2;
    public static int LAYER_PLAYER = 3;
    public static int LAYER_WATER = 4;
    public static int LAYER_UI = 5;
    public static int LAYER_GROUND = 6;
    public static int LAYER_PLAYERCAMERAIGNORE = 7;
    public static int LAYER_FPSLAYER = 8;
    public static int LAYER_RAGDOLL = 9;

    //------------ MAPS IDs in Build Settings --------------
    public static int  MAP_MAINMENU = 0;
    public static int MAP_FPSTESTMAP = 1;
    public static int MAP_DISCOVERY = 2;
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
    public static string ANIM1_TRIGGER_SHOOTLAST = "ShootLastBullet";
    public static string ANIM1_SPECIALATTACK = "Special Attack";
    public static string ANIM1_TRIGGER_RELOAD = "Reload";
    //Shotguns
    public static string ANIM1_BOOL_RELOADING = "Reloading";
    public static string ANIM1_TRIGGER_FINISHRELOAD = "FinishReload";
    public static string ANIM1_TRIGGER_STARTRELOAD = "StartReload";

    public static string ANIM1_TRIGGER_SHOWWEAPON = "ShowWeapon";
    public static string ANIM1_TRIGGER_HIDEWEAPON = "HideWeapon";
    public static string ANIM1_BOOLEAN_SHOOTING = "Shooting";
    public static string ANIM1_BOOLEAN_AIM = "Aim";
    public static string ANIM1_FLOAT_WALK = "Walk";
    public static string ANIM1_TAUNT = "Taunt";


}
