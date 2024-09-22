using System.Collections;
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
}

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Button startButton;
    public GameObject playerPrefab, countdownPanel, settingsPanel, roomDetails;
    public TMP_Text roomText, countdownText, playerCountText, hostText, roomStatusText, nameText;
    public TMP_Dropdown topicDropdown;
    [Space]
    public Sprite[] lockSprites;
    public Image[] statBars, lockImages;
    public TMP_Text[] statTexts;

    [Header("Settings")]
    public string[] minigames;
    public float startTime = 10.0f;
    public float minigameTime = 5f;
    public Vector2[] positionBounds = new Vector2[2];

    [Header("Debug")]
    public MinigameScenes debugGame = MinigameScenes.TerritoryConquest;
    public TMP_Dropdown debugTopicDropdown;
    public Button debugButton;

    int seed;
    ExitGames.Client.Photon.Hashtable roomOptions = new();

    private void Awake()
    {
        #if UNITY_EDITOR
        GameObject newObject = new("SaveManager");
        newObject.AddComponent<SaveManager>();
        SaveManager.saveFolder ??= System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        SaveManager.player ??= SaveManager.LoadPlayer(0);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = SaveManager.player.profile.name;
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.JoinOrCreateRoom("test", LobbyManager.roomOptions, Photon.Realtime.TypedLobby.Default);
        }
        #endif

        #if DEBUG
        if (PhotonNetwork.IsMasterClient && PlayerPrefs.GetInt("DebugMode", 0) == 1) debugButton.gameObject.SetActive(true);
        #endif
    }

    private void Start() 
    {

        if (PhotonNetwork.IsMasterClient) 
        {
            topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            PhotonNetwork.MasterClient.NickName = SaveManager.player.profile.name;
        }
        else 
        {
            topicDropdown.interactable = false;
            startButton.interactable = false;
        }

        SpawnPlayers();
        UpdatePlayerInterface();
        UpdateRoomDetails();

        #if DEBUG
        debugTopicDropdown.value = (int)debugGame;
        debugTopicDropdown.onValueChanged.AddListener(OnDebugDropdownValueChanged);
        #endif
    }

    private void OnDebugDropdownValueChanged(int arg0)
    {
        debugGame = (MinigameScenes)arg0;
    }

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) 
    { 
        UpdateRoomDetails();
        PlayerCountCheck();

    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdateRoomDetails();
        PlayerCountCheck();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) 
    { 
        HostCheck(); 
        PlayerCountCheck();
    }

    #endregion

    #region Countdown Function

    public void TriggerCountdown() 
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.interactable = false;
            topicDropdown.interactable = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            int num = Random.Range(0, minigames.Length);
            photonView.RPC("SetSeedRPC", RpcTarget.All, num);
            photonView.RPC("StartCountdown", RpcTarget.All);
        }
    }

    [PunRPC] public void SetSeedRPC(int seed) { this.seed = seed; }

    [PunRPC]
    private void StartCountdown() 
    {
        UpdateRoomDetails();
        countdownPanel.SetActive(true);
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

        #if DEBUG
        string selectedGame = debugGame.ToString();
        #else
        string selectedGame = minigames[seed];
        #endif

        while (time > 0) 
        {
            countdownText.text = $"Chosen game is {Regex.Replace(selectedGame, "(?<=\\p{Ll})(?=\\p{Lu})", " ")}. Starting in {time}";
            yield return new WaitForSeconds(1);
            time--;
        }

        if (PhotonNetwork.IsMasterClient) RequestStartGame(selectedGame);
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

        string[] avatarAnimatorNames = { "Adam", "Alex", "Bob", "Amelia" };
        object[] instantiationData = new object[] { avatarAnimatorNames[SaveManager.player.profile.avatar] };
        PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity, 0, instantiationData);
    }

    private void UpdateRoomDetails() 
    {
        playerCountText.gameObject.SetActive(true);
        hostText.gameObject.SetActive(true);
        roomStatusText.gameObject.SetActive(true);
        roomText.text = $"Code: {PhotonNetwork.CurrentRoom.Name}";
        playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
        hostText.text = $"Host: {PhotonNetwork.MasterClient.NickName}";
        roomStatusText.text = $"Status: {(PhotonNetwork.CurrentRoom.IsOpen ? "Open" : "Closed")}";

        PhotonNetwork.LocalPlayer.NickName = $"{SaveManager.player.profile.name}";
        //if (!PhotonNetwork.IsMasterClient) photonView.RPC("RequestDropdownValue", RpcTarget.MasterClient);
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

        string topic = topicDropdown.value switch
        {
            0 => "HOC",
            1 => "EOCS",
            2 => "NS",
            3 => "ITP",
            _ => "HOC",
        };

        if (PhotonNetwork.InRoom) 
        {
            roomOptions["selectedTopic"] = topic;
            PhotonNetwork.LocalPlayer.CustomProperties = roomOptions;
        }
        else 
        {
            Debug.LogWarning("Not in a room. Cannot set custom property.");
        }
    }

    #endregion

    #region UI Functions

    public void UpdatePlayerInterface()
    {
        if (SaveManager.player.stats.isNumberSystemUnlocked) lockImages[0].sprite = lockSprites[0];
        else lockImages[0].sprite = lockSprites[1];

        if (SaveManager.player.stats.isIntroProgrammingUnlocked) lockImages[1].sprite = lockSprites[1];
        else lockImages[1].sprite = lockSprites[1];

        nameText.text = SaveManager.player.profile.name;

        float[] playerStats = {
            SaveManager.player.stats.computerHistory,
            SaveManager.player.stats.computerElements,
            SaveManager.player.stats.numberSystem,
            SaveManager.player.stats.introProgramming
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

    void PlayerCountCheck()
    {
        #if DEBUG 
        if (PhotonNetwork.IsMasterClient && PlayerPrefs.GetInt("DebugMode", 0) == 1) startButton.interactable = true;
        #else
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            startButton.interactable = true;
        else startButton.interactable = false;
        #endif
    }

    #endregion
}
