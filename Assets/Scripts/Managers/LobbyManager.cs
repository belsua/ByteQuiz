using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public GameObject errorPanel;

    public void ValidateAndCreate()
    {
        if (createInput.text.Length < 3) errorPanel.SetActive(true);
        else CreateRoom();
    }

    #region Photon

    public void CreateRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(createInput.text, new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom() {
        PhotonNetwork.LoadLevel("Room");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"{returnCode} {message}");
    }

    #endregion
}
