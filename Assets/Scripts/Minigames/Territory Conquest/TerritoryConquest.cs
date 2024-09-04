using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class TerritoryConquest : Minigame
{
    [Header("Minigame Variables")]
    public TileClaim tileClaim;
    public Vector2[] spawnPoints = new Vector2[4];
    public TMP_Text timeText, markText, scoreText;
    public int timer = 60;

    Dictionary<string, int> playerData = new(); // <player, score>
    internal int playerIndex;

    #region Game Loop

    // Initialization and start

    public override void SpawnPlayers()
    {
        playerPrefab.GetComponent<SpriteRenderer>().sortingOrder = playerSpriteOrder;
        playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[playerIndex], Quaternion.identity);
    }

    public override void StartMinigame()
    {
        AudioSource.Play();

        score = 0;
        ReceiveSelectedTopic();
        GenerateQuestions();
        StartCoroutine(StartTimerCoroutine());

        InitializePlayerData();
    }

    IEnumerator StartTimerCoroutine()
    {
        while (timer >= 0)
        {
            timeText.text = $"Time left: {timer}";
            yield return new WaitForSeconds(1);
            timer--;
        }

        if (PhotonNetwork.IsMasterClient) photonView.RPC("EndGameRPC", RpcTarget.All);
    }

    public override void InitializePlayerData()
    {
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        foreach (Photon.Realtime.Player player in players)
            playerData.Add(player.NickName, 0);
    }

    // Quiz function

    public override void AnswerCorrect()
    {
        correct++;
        tileClaim.ClaimTile();
        ChangeScoreList(playerName, score);

        AudioManager.PlaySound(correctClip);
        markText.text = "Correct!";
        StartCoroutine(ClearMarkText());
        UnfreezePlayer();
        quizPanel.SetActive(false);

        RemoveQuestion(currentQuestionIndex);
        GenerateQuestions();
    }

    public override void AnswerWrong()
    {
        AudioManager.PlaySound(wrongClip);
        markText.text = "Wrong!";
        StartCoroutine(ClearMarkText());
        UnfreezePlayer();
        quizPanel.SetActive(false);

        RemoveQuestion(currentQuestionIndex);
        GenerateQuestions();
    }
    IEnumerator ClearMarkText()
    {
        yield return new WaitForSeconds(1f);
        markText.text = string.Empty;
    }

    // End game

    [PunRPC] 
    private void EndGameRPC()
    {
        StartCoroutine(EndGameCoroutine());
    }

    IEnumerator EndGameCoroutine()
    {
        tileClaim.GetComponent<TilemapCollider2D>().enabled = false;
        quizPanel.GetComponent<Image>().enabled = false;
        quizPanel.SetActive(true);
        questionImage.gameObject.SetActive(false);
        buttons.SetActive(false);
        timeText.text = string.Empty;
        questionText.text = "Finished!";
        UnfreezePlayer();
        AudioManager.PlaySound(finishClip);
        yield return new WaitForSeconds(5);
        EndMinigame();
    }

    public override void EndMinigame()
    {
        // Increase stats
        SaveManager.player.IncreaseStat(topic, score / 100f);

        // Handle UI
        AudioSource.Stop();

        StartCoroutine(DisplayScores());
    }

    // Handle player place
    IEnumerator DisplayScores()
    {
        // Show the ranking in the text
        AudioManager.PlaySound(roundClip);
        questionText.text = string.Empty;
        foreach (var entry in playerData) questionText.text += $"{entry.Key}: {entry.Value}\n";
        yield return new WaitForSeconds(5.0f);

        // Increase stats
        StartCoroutine(NotifyIncrease());
    }

    IEnumerator NotifyIncrease()
    {
        questionText.transform.parent.gameObject.SetActive(false);
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
        yield return new WaitForSeconds(5.0f);
        StartCoroutine(LoadLobby());
    }

    IEnumerator LoadLobby()
    {
        int timeLeft = returnTime;
        while (timeLeft > 0)
        {
            messageText.text = $"Going to lobby in {timeLeft}...";
            yield return new WaitForSeconds(1);
            timeLeft--;
        }

        PhotonNetwork.LoadLevel("Room");
    }

    #endregion

    #region Game Data

    public void ChangeScoreList(string player, int score)
    {
        photonView.RPC("UpdateScoreList", RpcTarget.All, player, score);
    }

    [PunRPC]
    public void UpdateScoreList(string player, int score)
    {
        playerData[player] = score;
    }

    #endregion
}
