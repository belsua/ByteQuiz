using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public Button startButton;
    public GameObject playerPrefab, countdownPanel, settingsPanel;
    public TMP_Text roomText, countdownText;
    public TMP_Dropdown topicDropdown;

    public float startTime = 10.0f;
    private float currentTime;
    private bool timerStarted = false;
    string selectedTopic;
    string[] minigames = { "Runner" };

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private ExitGames.Client.Photon.Hashtable roomOptions = new();

    private void Start()
    {
        SpawnPlayers();
        UpdateRoomDetails();

        if (PhotonNetwork.IsMasterClient)
        {
            topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        else
        {
            topicDropdown.interactable = false;
            startButton.interactable = false;
        }
    }

    private void Update()
    {
        if (timerStarted)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                countdownText.text = $"Game starts! Choosing a game in {Mathf.Ceil(currentTime)}";
            }
            else
            {
                timerStarted = false;
                StartCoroutine(SelectMinigame());
            }
        }
    }

    #region Minigame Initiation Functions
    
    IEnumerator SelectMinigame()
    {
        int time = 5;
        string selectedGame = minigames[Random.Range(0, minigames.Length)];

        while (time > 0)
        {
            countdownText.text = $"Chosen game is {selectedGame}. Starting in {time}";
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
        PhotonNetwork.LoadLevel(selectedGame);
    }

    #endregion

    #region Room Synchronization Functions

    void SpawnPlayers()
    {
        Vector2 position = new(Random.Range(minX, maxX), Random.Range(minY, maxY));
        PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
    }

    private void UpdateRoomDetails()
    {
        roomText.text = PhotonNetwork.CurrentRoom.Name;
        PhotonNetwork.LocalPlayer.NickName = SaveManager.instance.player.name;
        if (!PhotonNetwork.IsMasterClient) photonView.RPC("RequestDropdownValue", RpcTarget.MasterClient);
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

    #region Countdown Function

    public void TriggerCountdown()
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("StartCountdown", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void StartCountdown()
    {
        currentTime = startTime;
        countdownPanel.SetActive(true);
        settingsPanel.SetActive(false);
        timerStarted = true;
    }

    #endregion
}
