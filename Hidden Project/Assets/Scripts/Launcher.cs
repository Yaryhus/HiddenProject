﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;

    private void Awake()
    {
        Instance = this;
    }

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

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_LOADING);
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_MAINMENU);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform t in roomListContent)
        {
            Destroy(t.gameObject);
        }

        foreach(RoomInfo r in roomList)
        {
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(r);
        }
    }
}
