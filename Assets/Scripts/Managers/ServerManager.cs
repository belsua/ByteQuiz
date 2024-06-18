using Photon.Pun;
using UnityEngine;
using TMPro;

public class ServerManager : MonoBehaviourPunCallbacks
{
    public GameObject loadingCanvas, lobbyCanvas;
    public TMP_Text regionText;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            SwitchPanel();
            UpdateRegionText();
        }
    }

    public override void OnConnectedToMaster()
    {
        UpdateRegionText();
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

    void UpdateRegionText()
    {
        string region = PhotonNetwork.CloudRegion;
        regionText.text = $"Connected to: {region}";
    }
}
