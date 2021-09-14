using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_MAINMENU);
        Debug.Log("Joined Lobby!");
    }
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_LOADING);
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_ROOM);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = ("Room Creation failed: " + message);
        //Debug.Log("Error joining room");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_LOADING);
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_MAINMENU);
    }
}
