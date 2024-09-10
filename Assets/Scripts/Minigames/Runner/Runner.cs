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
    [SerializeField] TMP_Text scoreText, standingsText, scoreListText;
    [SerializeField] GameObject standingsPanel, scorePanel;
    [SerializeField] Transform teleportLocation;

    Dictionary<string, PlayerData> playerData = new(); // <player name, player data>
    public GameObject currentObject;

    protected override void Awake()
    {
        base.Awake();
        scoreText.text  = string.Empty;
    }

    public override void StartMinigame() 
    {
        scoreText.transform.parent.gameObject.SetActive(true);
        AudioSource.Play();
        ReceiveSelectedTopic();
        GenerateQuestions();
        score = 0;
        InitializePlayerData();
        ChangeUI_RPC();
    }

    [PunRPC]
    private void UpdateUI() 
    {
        scoreText.text = $"Score: {score}";
        scoreListText.text = string.Empty;
        var sortedPlayerData = playerData.OrderByDescending(x => x.Value.score);

        #if UNITY_EDITOR 
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

    public void ChangeUI_RPC() 
    {
        photonView.RPC("UpdateUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) 
    {
        playerData.Remove(otherPlayer.NickName);
        ChangeUI_RPC();
    }

    #region Quiz Functions

    public override void AnswerCorrect() 
    {
        AudioManager.PlaySound(correctClip);
        score = Mathf.Clamp(score + 100, 0, 1000);
        ChangeScoreList(playerName, score);
        ChangeUI_RPC();
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
        ChangeUI_RPC();
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
        ChangeUI_RPC();
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
        photonView.RPC("EndMinigame", RpcTarget.All);
    }

    [PunRPC]
    public override void EndMinigame()
    {
        base.EndMinigame();
        standingsText.text = string.Empty;
        foreach (var entry in playerData) standingsText.text += $"{entry.Key}: {entry.Value.score}\n";
        AudioSource.Stop();
        quizPanel.SetActive(false);
        scorePanel.SetActive(false);
        AudioManager.PlaySound(roundClip);
        StartCoroutine(UpdateScores(time: 5));
    }

    IEnumerator UpdateScores(int time)
    {
        standingsPanel.SetActive(true);
        yield return new WaitForSeconds(time);
        standingsPanel.SetActive(false);
        SaveManager.player.IncreaseStat(topic, score / 5000);
        NotifyIncrease();
    }

    #endregion

    #region Game Variables

    public override void InitializePlayerData()
    {
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
