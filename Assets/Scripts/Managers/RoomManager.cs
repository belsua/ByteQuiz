using System.IO;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Text.RegularExpressions;

public enum MinigameScenes
{
    Runner,
    TriviaShowdown,
    Random
}

public class RoomManager : MonoBehaviourPunCallbacks 
{
    [Header("UI")]
    public Button startButton;
    public GameObject playerPrefab, countdownPanel, settingsPanel;
    public TMP_Text roomText, countdownText;
    public TMP_Dropdown topicDropdown;

    [Header("Settings")]
    public float startTime = 10.0f;
    public float minigameTime = 5f;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    #if UNITY_EDITOR
    [Header("Debug")]
    public MinigameScenes debugGame;
    public TMP_Text debugText;
    public TMP_Text hostText;
    public TMP_Text roomStatusText;
    #endif

    string[] minigames = new string[0];
    string scenePath = "Assets/Scenes/Multiplayer/Minigames";
    ExitGames.Client.Photon.Hashtable roomOptions = new();

    private void Start() 
    {
        GetMinigames();

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

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        ShowRoomDetails();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        ShowRoomDetails();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) {
        HostCheck();
    }

    #endregion

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
        string selectedGame = debugGame.ToString();
        #else
        string selectedGame = minigames[Random.Range(0, minigames.Length)];
        #endif

        while (time > 0) {
            countdownText.text = $"Chosen game is {Regex.Replace(selectedGame, "(?<=\\p{Ll})(?=\\p{Lu})", " ")}. Starting in {time}";
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
        if (SaveManager.player.name == "") PhotonNetwork.LocalPlayer.NickName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}";
        else PhotonNetwork.LocalPlayer.NickName = $"{SaveManager.player.name}";
        #else
        PhotonNetwork.LocalPlayer.NickName = $"{SaveManager.player.name}";
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

    #region UI Functions

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

    private void GetMinigames()
    {
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { scenePath });
        minigames = new string[sceneGUIDs.Length];

        for (int i = 0; i < sceneGUIDs.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUIDs[i]);
            minigames[i] = Path.GetFileNameWithoutExtension(scenePath);
        }
    }

    #endregion
}
