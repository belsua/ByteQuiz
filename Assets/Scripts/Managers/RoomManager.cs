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
    TerritoryConquest,
    Random
}

public class RoomManager : MonoBehaviourPunCallbacks 
{
    [Header("UI")]
    public Button startButton;
    public GameObject playerPrefab, countdownPanel, settingsPanel, roomDetails;
    public TMP_Text roomText, countdownText, playerCountText, hostText, roomStatusText, nameText;
    public TMP_Dropdown topicDropdown;
    [Space]
    public Image[] statBars;
    public TMP_Text[] statTexts, statusTexts;

    [Header("Settings")]
    public float startTime = 10.0f;
    public float minigameTime = 5f;
    public Vector2[] positionBounds = new Vector2[2];

    #if UNITY_EDITOR
    [Header("Debug")]
    public MinigameScenes debugGame = MinigameScenes.Random;
    #endif

    string[] minigames = new string[0];
    string scenePath = "Assets/Scenes/Multiplayer/Minigames";
    ExitGames.Client.Photon.Hashtable roomOptions = new();

    private void Start() 
    {
        roomDetails.SetActive(false);

        if (PhotonNetwork.IsMasterClient) 
        {
            topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            PhotonNetwork.MasterClient.NickName = SaveManager.player.name;
        }
        else 
        {
            topicDropdown.interactable = false;
            startButton.interactable = false;
        }

        GetMinigames();
        SpawnPlayers();
        UpdatePlayerInterface();
        UpdateRoomDetails();
    }

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) { UpdateRoomDetails(); }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) { UpdateRoomDetails(); }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) { HostCheck(); }

    #endregion

    #region Countdown Function

    public void TriggerCountdown() 
    {
        if (PhotonNetwork.IsMasterClient) photonView.RPC("StartCountdown", RpcTarget.All);
    }

    [PunRPC]
    private void StartCountdown() 
    {
        countdownPanel.SetActive(true);
        settingsPanel.SetActive(false);
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown() 
    {
        int time = (int)startTime;
        while (time > 0) 
        {
            countdownText.text = $"Game starts! Choosing a game in {time}";
            yield return new WaitForSeconds(1);
            time--;
        }

        StartCoroutine(SelectMinigame());
    }

    #endregion

    #region Minigame Initiation Functions

    IEnumerator SelectMinigame() 
    {
        int time = (int)minigameTime;

        #if UNITY_EDITOR
        string selectedGame = debugGame.ToString();
        #else
        string selectedGame = minigames[Random.Range(0, minigames.Length)];
        #endif

        while (time > 0) 
        {
            countdownText.text = $"Chosen game is {Regex.Replace(selectedGame, "(?<=\\p{Ll})(?=\\p{Lu})", " ")}. Starting in {time}";
            yield return new WaitForSeconds(1);
            time--;
        }

        RequestStartGame(selectedGame);
    }

    public void RequestStartGame(string selectedGame) 
    {
        photonView.RPC("StartGame", RpcTarget.All, selectedGame);
    }

    [PunRPC]
    public void StartGame(string selectedGame) 
    {
        SetSelectedTopic();
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LoadLevel(selectedGame);
    }

    #endregion

    #region Room Synchronization Functions

    void SpawnPlayers() 
    {
        Vector2 position = new(
            Random.Range(positionBounds[0].x, positionBounds[1].x),
            Random.Range(positionBounds[0].y, positionBounds[1].y)
        );

        PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
    }

    private void UpdateRoomDetails() 
    {
        playerCountText.gameObject.SetActive(true);
        hostText.gameObject.SetActive(true);
        roomStatusText.gameObject.SetActive(true);
        roomText.text = $"Code: {PhotonNetwork.CurrentRoom.Name}";
        playerCountText.text = $"Player Count: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
        hostText.text = $"Host: {PhotonNetwork.MasterClient.NickName}";
        roomStatusText.text = $"Status: {(PhotonNetwork.CurrentRoom.IsOpen ? "Open" : "Closed")}";

        #if UNITY_EDITOR
        if (SaveManager.player == null) PhotonNetwork.LocalPlayer.NickName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}";
        else PhotonNetwork.LocalPlayer.NickName = $"{SaveManager.player.name}";
        #else
        PhotonNetwork.LocalPlayer.NickName = $"{SaveManager.player.name}";
        if (!PhotonNetwork.IsMasterClient) photonView.RPC("RequestDropdownValue", RpcTarget.MasterClient);
        #endif
    }

    [PunRPC]
    void RequestDropdownValue() 
    {
        photonView.RPC("UpdateDropdownValue", RpcTarget.Others, topicDropdown.value);
    }

    [PunRPC]
    void UpdateDropdownValue(int value) 
    {
        topicDropdown.value = value;
    }

    void OnDropdownValueChanged(int value) 
    {
        photonView.RPC("UpdateDropdownValue", RpcTarget.All, value);
    }

    public void SetSelectedTopic() 
    {
        if (PhotonNetwork.InRoom) 
        {
            roomOptions["selectedTopic"] = GetTopic();
            PhotonNetwork.LocalPlayer.CustomProperties = roomOptions;
        }
        else 
        {
            Debug.LogWarning("Not in a room. Cannot set custom property.");
        }
    }
    string GetTopic() => topicDropdown.value switch 
    {
        0 => "HOC",
        1 => "EOCS",
        2 => "NS",
        3 => "ITP",
        _ => "HOC",
    };

    #endregion

    #region UI Functions

    public void UpdatePlayerInterface()
    {
        if (SaveManager.player.isNumberSystemUnlocked) statusTexts[0].transform.parent.gameObject.SetActive(false);
        else statusTexts[0].text = "Locked";

        if (SaveManager.player.isIntroProgrammingUnlocked) statusTexts[1].transform.parent.gameObject.SetActive(false);
        else statusTexts[1].text = "Locked";

        nameText.text = SaveManager.player.name;

        float[] playerStats = {
            SaveManager.player.computerHistory,
            SaveManager.player.computerElements,
            SaveManager.player.numberSystem,
            SaveManager.player.introProgramming
        };

        for (int i = 0; i < playerStats.Length; i++)
        {
            statBars[i].fillAmount = playerStats[i];
            statTexts[i].text = ((int)(playerStats[i] * 10000)).ToString();
        }
    }

    public void Toggle()
    {
        if (roomDetails != null) roomDetails.SetActive(!roomDetails.activeSelf);
    }

    void HostCheck() 
    {
        if (PhotonNetwork.IsMasterClient) 
        {
            startButton.interactable = true;
            topicDropdown.interactable = true;
        }
        else 
        {
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
