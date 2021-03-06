using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{

    public static Launcher Instance;
    [SerializeField] int mapToLoad;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject gameStartButton;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
       // Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        //Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_MAINMENU);
        //Debug.Log("Joined Lobby!");
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

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_LOADING);
    }


    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_ROOM);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        foreach(Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        Player[] players = PhotonNetwork.PlayerList;
        foreach (Player p in players)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(p);
        }

        gameStartButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        gameStartButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        //Debug.LogError("Room Creation Failed: " + message);
        MenuManager.Instance.OpenMenu(GlobalVariablesAndStrings.MENU_NAME_ERRORMENU);
       
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

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform t in roomListContent)
        {
            Destroy(t.gameObject);
        }

        foreach (RoomInfo r in roomList)
        {
            if (r.RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(r);
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }


    public void StartGame()
    {
        gameStartButton.SetActive(false);
        PhotonNetwork.LoadLevel(mapToLoad);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
