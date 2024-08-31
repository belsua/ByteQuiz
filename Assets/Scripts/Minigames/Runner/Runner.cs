using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData 
{
    public int score { get; set; }
    public bool isFinished { get; set; }
}

public class Runner : Minigame 
{
    [SerializeField] AudioClip roundClip, upClip;
    [SerializeField] TMP_Text scoreText, standingsText, scoreListText;
    [SerializeField] GameObject controls, standingsPanel, scorePanel;
    [SerializeField] Transform teleportLocation;

    string playerName;
    Dictionary<string, PlayerData> playerData = new();
    public GameObject currentObject;

    #if UNITY_EDITOR
    int returnTime = 1;
    #else
    readonly int returnTime = 5;
    #endif

    protected override void Awake()
    {
        base.Awake();
        scoreText.text  = string.Empty;
    }

    public override void StartGame() 
    {
        scoreText.transform.parent.gameObject.SetActive(true);
        AudioSource.Play();
        ReceiveSelectedTopic();
        GenerateQuestions();
        score = 0;
        InitializePlayerData();
        ChangeUI();
    }

    [PunRPC]
    private void UpdateUI() 
    {
        scoreText.text = $"Score: {score}";
        scoreListText.text = string.Empty;
        var sortedPlayerData = playerData.OrderByDescending(x => x.Value.score);

        #if DEVELOPMENT_BUILD 
        foreach (var entry in sortedPlayerData) 
        {
            scoreListText.text += $"{entry.Key}: {entry.Value.score}, {entry.Value.isFinished}\n";
        }
        #else
        foreach (var entry in sortedPlayerData) 
        {
            scoreListText.text += $"{entry.Key}: {entry.Value.score}\n";
        }
        #endif
    }

    public void ChangeUI() 
    {
        photonView.RPC("UpdateUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) 
    {
        playerData.Remove(otherPlayer.NickName);
        ChangeUI();
    }

    #region Quiz Functions

    public override void AnswerCorrect() 
    {
        AudioManager.PlaySound(correctClip);
        score = Mathf.Clamp(score + 100, 0, 1000);
        ChangeScoreList(playerName, score);
        ChangeUI();
        RemoveQuestion(currentQuestionIndex);
        Destroy(currentObject);
        GenerateQuestions();
        ToggleQuiz(false);
    }

    public override void AnswerWrong() 
    {
        AudioManager.PlaySound(wrongClip);
        score = Mathf.Clamp(score - 20, 0, 1000);
        ChangeScoreList(playerName, score);
        ChangeUI();
    }

    public void ToggleQuiz(bool state) 
    {
        quizPanel.SetActive(state);
        controls.SetActive(!state);
    }

    #endregion

    #region Finish Line Functions

    private void OnTriggerEnter2D(Collider2D other) 
    {
        AudioManager.PlaySound(finishClip);
        other.transform.position = teleportLocation.position;
        playerData[playerName].isFinished = true;
        ChangeFinishPlayer(playerName);
        ChangeUI();
        CheckIfAllPlayersFinished();
    }

    public override void OnDisconnected(DisconnectCause cause) 
    {
        Debug.LogWarning("Disconnected from Photon with cause: " + cause);
        SceneManager.LoadScene("Lobby");
    }

    public void CheckIfAllPlayersFinished() 
    {
        if (playerData.Values.All(x => x.isFinished)) StartCoroutine(EndGameCoroutine());
    }

    IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(2);
        RPCEndGame();
    }

    public void RPCEndGame()
    {
        photonView.RPC("EndGame", RpcTarget.All);
    }

    [PunRPC]
    public override void EndGame()
    {
        SaveManager.selectedPlayer.IncreaseStat(topic, score / 200);
        standingsText.text = string.Empty;
        foreach (var entry in playerData) standingsText.text += $"{entry.Key}: {entry.Value.score}\n";
        AudioSource.Stop();
        quizPanel.SetActive(false);
        scorePanel.SetActive(false);
        AudioManager.PlaySound(roundClip);
        StartCoroutine(DisplayScores(time: 5));
    }

    IEnumerator DisplayScores(int time)
    {
        standingsPanel.SetActive(true);
        yield return new WaitForSeconds(time);
        standingsPanel.SetActive(false);
        StartCoroutine(NotifyIncrease(time: 3));
    }

    IEnumerator NotifyIncrease(int time)
    {
        messagePanel.SetActive(true);

        string formattedTopic = topic switch
        {
            "HOC" => "computer history",
            "EOCS" => "computer elements",
            "NS" => "number system",
            "ITP" => "intro to programming",
            _ => topic,
        };

        messageText.text = $"Your {formattedTopic} stat is increased!";
        AudioManager.PlaySound(upClip);
        yield return new WaitForSeconds(time);
        StartCoroutine(LoadLobby(time: returnTime));
    }

    IEnumerator LoadLobby(int time)
    {
        int timeLeft = time;
        while (timeLeft > 0) {
            messageText.text = $"Going to lobby in {timeLeft}...";
            yield return new WaitForSeconds(1);
            timeLeft--;
        }
        // PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LoadLevel("Room");
    }

    #endregion

    #region Game Variables

    public void InitializePlayerData()
    {
        playerName = PhotonNetwork.LocalPlayer.NickName;
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        foreach (Photon.Realtime.Player player in players)
            playerData.Add(player.NickName, new PlayerData { score = 0, isFinished = false });
    }

    [PunRPC]
    public void UpdateScoreList(string player, int score) 
    {
        playerData[player].score = score;
    }

    public void ChangeScoreList(string player, int score) 
    {
        photonView.RPC("UpdateScoreList", RpcTarget.All, player, score);
    }

    [PunRPC]
    public void UpdateFinishPlayer(string player)
    {
        playerData[player].isFinished = true;
    }

    public void ChangeFinishPlayer(string player)
    {
        photonView.RPC("UpdateFinishPlayer", RpcTarget.All, player);
    }

    #endregion
}
