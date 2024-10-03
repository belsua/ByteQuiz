using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Text.RegularExpressions;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using System.Linq;

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
    public TMP_Dropdown debugGameDropdown;
    public Button debugButton;
    MinigameScenes debugGame = MinigameScenes.Runner;

    bool[] optionEnabled;
    int seed;
    Hashtable roomOptions = new();

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
        topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        if (PhotonNetwork.IsMasterClient) 
        {
            PhotonNetwork.MasterClient.NickName = SaveManager.player.profile.name;
        }
        else 
        {
            photonView.RPC("RequestDropdownValue", RpcTarget.MasterClient);
            topicDropdown.interactable = false;
            startButton.interactable = false;
        }

        SpawnPlayers();
        UpdatePlayerInterface();
        UpdateRoomDetails();

        #if DEBUG
        debugGameDropdown.value = (int)debugGame;
        debugGameDropdown.onValueChanged.AddListener(OnDebugDropdownValueChanged);
        #endif
    }

    private bool[] GetTopicDenominator()
    {
        List<bool> numberSystemUnlocked = new();
        List<bool> introProgrammingUnlocked = new();

        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("IsNumberSystemUnlocked", out object numberSystemStatus))
                numberSystemUnlocked.Add((bool)numberSystemStatus);
            else
                numberSystemUnlocked.Add(false);

            if (player.CustomProperties.TryGetValue("IsIntroProgrammingUnlocked", out object introProgrammingStatus))
                introProgrammingUnlocked.Add((bool)introProgrammingStatus);
            else
                introProgrammingUnlocked.Add(false);
        }

        bool isNumberSystemUnlocked = numberSystemUnlocked.All(unlocked => unlocked);
        bool isIntroProgrammingUnlocked = introProgrammingUnlocked.All(unlocked => unlocked);

        return new bool[] { true, true, isNumberSystemUnlocked, isIntroProgrammingUnlocked };
    }

    private void UpdateDropdownOptions()
    {
        optionEnabled = GetTopicDenominator();

        Debug.Log(string.Join(", ", optionEnabled));
        if (PhotonNetwork.IsMasterClient && (topicDropdown.value == 2 || topicDropdown.value == 3)) topicDropdown.value = GetFirstEnabledOption();

        for (int i = 0; i < topicDropdown.options.Count; i++)
        {
            TMP_Dropdown.OptionData optionData = topicDropdown.options[i];
            string optionText = optionData.text;

            if (!optionEnabled[i])
            {
                if (!optionText.Contains("(Not All Unlocked)"))
                {
                    optionText += " (Not All Unlocked)";
                }
            }
            else
            {
                optionText = optionText.Replace(" (Not All Unlocked)", string.Empty);
            }

            optionData.text = optionText;
        }

        topicDropdown.RefreshShownValue();
    }

    #region Debug

    private void OnDebugDropdownValueChanged(int index)
    {
        photonView.RPC("UpdateDebugDropdownValue", RpcTarget.All, index);
    }

    [PunRPC] public void UpdateDebugDropdownValue(int value) { debugGame = (MinigameScenes)value; }

    #endregion

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) 
    { 
        UpdateDropdownOptions();
        UpdateRoomDetails();
        PlayerCountCheck();

    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdateDropdownOptions();
        UpdateRoomDetails();
        PlayerCountCheck();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) 
    {
        UpdateDropdownOptions();
        HostCheck(); 
        PlayerCountCheck();
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log($"Player {targetPlayer.NickName} changed props: {changedProps}");
        UpdateDropdownOptions();
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

        Hashtable playerUnlockProperties = new()
        {
            { "IsNumberSystemUnlocked", SaveManager.player.stats.isNumberSystemUnlocked },
            { "IsIntroProgrammingUnlocked", SaveManager.player.stats.isIntroProgrammingUnlocked }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerUnlockProperties);
    }

    private void UpdateRoomDetails() 
    {
        roomDetails.GetComponent<SizeAnimate>().Open();

        playerCountText.gameObject.SetActive(true);
        hostText.gameObject.SetActive(true);
        roomStatusText.gameObject.SetActive(true);
        roomText.text = $"Code: {PhotonNetwork.CurrentRoom.Name}";
        playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
        hostText.text = $"Host: {PhotonNetwork.MasterClient.NickName}";
        roomStatusText.text = $"Status: {(PhotonNetwork.CurrentRoom.IsOpen ? "Open" : "Closed")}";

        PhotonNetwork.LocalPlayer.NickName = $"{SaveManager.player.profile.name}";
    }

    [PunRPC]
    void RequestDropdownValue() 
    {
        photonView.RPC("UpdateDropdownValue", RpcTarget.Others, topicDropdown.value);
    }

    [PunRPC]
    void UpdateDropdownValue(int value) 
    {
        if (!optionEnabled[value]) topicDropdown.value = GetFirstEnabledOption();
        else topicDropdown.value = value;
    }

    void OnDropdownValueChanged(int value) 
    {
        photonView.RPC("UpdateDropdownValue", RpcTarget.All, value);
    }

    private int GetFirstEnabledOption()
    {
        for (int i = 0; i < optionEnabled.Length; i++) if (optionEnabled[i]) return i;
        return 0;
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
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomOptions);
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

    void HostCheck() 
    {
        if (PhotonNetwork.IsMasterClient) topicDropdown.interactable = true;
        else topicDropdown.interactable = false;
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
