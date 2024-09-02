using Photon.Pun;
using UnityEngine;

public class ServerManager : MonoBehaviourPunCallbacks
{
    public GameObject loadingCanvas, lobbyCanvas;

    void Start()
    {
        lobbyCanvas.SetActive(false);
        loadingCanvas.transform.position = Vector3.zero;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            SwitchPanel();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        SwitchPanel();
    }

    void SwitchPanel()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            loadingCanvas.SetActive(false);
            lobbyCanvas.SetActive(true);
        }
    }
}
