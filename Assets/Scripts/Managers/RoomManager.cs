using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks 
{
    public Button startButton;
    public GameObject playerPrefab, countdownPanel, settingsPanel;
    public TMP_Text roomText, countdownText;
    public TMP_Dropdown topicDropdown;

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string debugGame;
    #endif

    public float startTime = 10.0f;
    public float minigameTime = 5f;
    string[] minigames = { "Runner", "Trivia Showdown" };

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    #if UNITY_EDITOR
    public TMP_Text debugText;
    public TMP_Text hostText;
    public TMP_Text roomStatusText;
    #endif

    private ExitGames.Client.Photon.Hashtable roomOptions = new();

    private void Start() {
        SpawnPlayers();
        UpdateRoomDetails();

        #if UNITY_EDITOR
        ShowRoomDetails();
        #endif

        if (PhotonNetwork.IsMasterClient) {
            topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        else {
            topicDropdown.interactable = false;
            startButton.interactable = false;
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        ShowRoomDetails();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        ShowRoomDetails();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) {
        HostCheck();
    }

    #region Countdown Function

    public void TriggerCountdown() {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("StartCountdown", RpcTarget.All);
    }

    [PunRPC]
    private void StartCountdown() {
        countdownPanel.SetActive(true);
        settingsPanel.SetActive(false);
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown() {
        int time = (int)startTime;
        while (time > 0) {
            countdownText.text = $"Game starts! Choosing a game in {time}";
            yield return new WaitForSeconds(1);
            time--;
        }

        StartCoroutine(SelectMinigame());
    }

    #endregion

    #region Minigame Initiation Functions

    IEnumerator SelectMinigame() {
        int time = (int)minigameTime;

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        string selectedGame = debugGame;
        #else
        string selectedGame = minigames[Random.Range(0, minigames.Length)];
        #endif

        while (time > 0) {
            countdownText.text = $"Chosen game is {selectedGame}. Starting in {time}";
            yield return new WaitForSeconds(1);
            time--;
        }

        RequestStartGame(selectedGame);
    }

    public void RequestStartGame(string selectedGame) {
        photonView.RPC("StartGame", RpcTarget.All, selectedGame);
    }

    [PunRPC]
    public void StartGame(string selectedGame) {
        SetSelectedTopic();
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LoadLevel(selectedGame);
    }

    #endregion

    #region Room Synchronization Functions

    void SpawnPlayers() {
        Vector2 position = new(Random.Range(minX, maxX), Random.Range(minY, maxY));
        PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
    }

    private void UpdateRoomDetails() {
        roomText.text = PhotonNetwork.CurrentRoom.Name;
        #if UNITY_EDITOR
        if (PhotonNetwork.LocalPlayer.NickName == "")
            PhotonNetwork.LocalPlayer.NickName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}";
        #else
        PhotonNetwork.LocalPlayer.NickName = SaveManager.instance.player.name;
        if (!PhotonNetwork.IsMasterClient) photonView.RPC("RequestDropdownValue", RpcTarget.MasterClient);
        #endif
    }

    [PunRPC]
    void RequestDropdownValue() {
        photonView.RPC("UpdateDropdownValue", RpcTarget.Others, topicDropdown.value);
    }

    [PunRPC]
    void UpdateDropdownValue(int value) {
        topicDropdown.value = value;
    }

    void OnDropdownValueChanged(int value) {
        photonView.RPC("UpdateDropdownValue", RpcTarget.All, value);
    }

    public void SetSelectedTopic() {
        if (PhotonNetwork.InRoom) {
            roomOptions["selectedTopic"] = GetTopic();
            PhotonNetwork.LocalPlayer.CustomProperties = roomOptions;
        }
        else {
            Debug.LogWarning("Not in a room. Cannot set custom property.");
        }
    }

    string GetTopic() => topicDropdown.value switch {
        0 => "HOC",
        1 => "EOCS",
        2 => "NS",
        3 => "ITP",
        _ => "HOC",
    };

#endregion

    void ShowRoomDetails() {
#if UNITY_EDITOR
            debugText.gameObject.SetActive(true);
            hostText.gameObject.SetActive(true);
            roomStatusText.gameObject.SetActive(true);
            debugText.text = $"Player Count: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
            hostText.text = $"Host: {PhotonNetwork.MasterClient.NickName}\n";
            hostText.text += $"In Room: {PhotonNetwork.InRoom}";
            roomStatusText.text = $"Room Status: {PhotonNetwork.CurrentRoom.IsOpen}";
#endif
    }

    void HostCheck() {
        if (PhotonNetwork.IsMasterClient) {
            startButton.interactable = true;
            topicDropdown.interactable = true;
        }
        else {
            startButton.interactable = false;
            topicDropdown.interactable = false;
        }
    }
}
