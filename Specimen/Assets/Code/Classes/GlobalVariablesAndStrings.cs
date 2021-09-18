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

    //------------ MAPS IDs in Build Settings --------------
    public static int  MAP_MAINMENU = 0;
    public static int MAP_FPSTESTMAP = 1;

}
