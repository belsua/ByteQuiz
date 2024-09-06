using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput, joinInput;
    public TMP_Text errorText;
    public GameObject errorPanel, loadingCanvas, lobbyCanvas;
    private List<RoomInfo> cachedRoomList = new();
    public static RoomOptions roomOptions = new()
    {
        MaxPlayers = 4,
        IsOpen = true,
    };

    private void Start()
    {
        lobbyCanvas.SetActive(false);

        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();

    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        loadingCanvas.SetActive(false);
        lobbyCanvas.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate: " + roomList.Count);
        
        cachedRoomList.Clear();
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) cachedRoomList.Remove(room);
            else cachedRoomList.Add(room);
        }
    }

    public void CreateRoom()
    {
        if (createInput.text.Length < 3) errorPanel.SetActive(true);
        else PhotonNetwork.JoinOrCreateRoom(createInput.text, roomOptions, TypedLobby.Default);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        loadingCanvas.SetActive(false);
        errorText.text = message;
        errorPanel.SetActive(true);
    }

    public void JoinRoom()
    {
        if (joinInput.text.Length < 3) errorPanel.SetActive(true);
        else CheckRoomStatus(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("Room");

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorText.text = message;
        errorPanel.SetActive(true);
    }

    #endregion

    private void CheckRoomStatus(string targetRoomName)
    {
        Debug.Log("Checking room status: " + targetRoomName);

        foreach (RoomInfo room in cachedRoomList)
        {
            if (room.Name == targetRoomName)
            {
                if (room.IsOpen)
                {
                    Debug.Log($"Room {room.Name} is open. You can join.");
                    PhotonNetwork.JoinRoom(joinInput.text);
                }
                else
                {
                    loadingCanvas.SetActive(false);
                    errorText.text = $"Room {targetRoomName} is currently playing.";
                    errorPanel.SetActive(true);
                    Debug.Log($"Room {room.Name} is closed.");
                }

                return;
            }
        }

        loadingCanvas.SetActive(false);
        errorText.text = $"Room {targetRoomName} does not exist.";
        errorPanel.SetActive(true);
        Debug.Log($"Room {targetRoomName} does not exist.");
    }
}
