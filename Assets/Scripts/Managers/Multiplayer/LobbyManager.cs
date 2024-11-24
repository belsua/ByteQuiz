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
        if (!PhotonNetwork.IsConnected)
        {
            loadingCanvas.SetActive(true);
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            loadingCanvas.SetActive(false);
            lobbyCanvas.GetComponentInChildren<SizeAnimate>().Open();
        }

    }
    
    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        loadingCanvas.SetActive(false);
        lobbyCanvas.GetComponentInChildren<SizeAnimate>().Open();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        cachedRoomList.Clear();
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) cachedRoomList.Remove(room);
            else cachedRoomList.Add(room);
        }
    }

    public void CreateRoom()
    {
        if (createInput.text.Length < 3)
        {
            errorPanel.GetComponent<SizeAnimate>().Open();
        }
        else
        {
            loadingCanvas.SetActive(true);
            PhotonNetwork.JoinOrCreateRoom(createInput.text, roomOptions, TypedLobby.Default);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        loadingCanvas.SetActive(false);
        errorText.text = message;
        errorPanel.GetComponent<SizeAnimate>().Open();
    }

    public void JoinRoom()
    {
        if (joinInput.text.Length < 3)
        {
            errorPanel.GetComponent<SizeAnimate>().Open();
        }
        else
        {
            loadingCanvas.SetActive(true);
            CheckRoomStatus(joinInput.text);
        }
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Room");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        loadingCanvas.SetActive(false);
        errorText.text = message;
        errorPanel.GetComponent<SizeAnimate>().Open();
    }

    #endregion

    private void CheckRoomStatus(string targetRoomName)
    {
        foreach (RoomInfo room in cachedRoomList)
        {
            if (room.Name == targetRoomName)
            {
                if (room.IsOpen)
                {
                    PhotonNetwork.JoinRoom(joinInput.text);
                }
                else
                {
                    loadingCanvas.SetActive(false);
                    errorText.text = $"Room {targetRoomName} is currently playing.";
                    errorPanel.GetComponent<SizeAnimate>().Open();
                }

                return;
            }
        }

        loadingCanvas.SetActive(false);
        errorText.text = $"Room {targetRoomName} does not exist.";
        errorPanel.GetComponent<SizeAnimate>().Open();
    }
}
