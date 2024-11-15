using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class TerritoryConquest : Minigame
{
    public class QuestionData : BaseQuestionData { }

    [Header("Minigame Variables")]
    public TileClaim tileClaim;
    public Vector2[] spawnPoints = new Vector2[4];
    public TMP_Text timeText, markText, scoreText;
    public int timer = 300; // in seconds

    Dictionary<string, QuestionData> answeredQuestions = new(); // <question number, question data>
    Dictionary<string, int> playerData = new(); // <player, score>
    internal int playerIndex;
    private int roundNumber = 0; // For keeping the answeredQuestions keys unique from resetting the questions

    #region Game Loop

    // Initialization and start

    public override void SpawnPlayers()
    {
        playerPrefab.GetComponent<SpriteRenderer>().sortingOrder = playerSpriteOrder;
        playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        string[] avatarAnimatorNames = { "Adam", "Alex", "Bob", "Amelia" };
        object[] instantiationData = new object[] { avatarAnimatorNames[SaveManager.player.profile.avatar] };
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[playerIndex], Quaternion.identity, 0, instantiationData);
    }

    public override void StartMinigame()
    {
        AudioSource.Play();
        score = 0;
        scoreText.text = $"Score: {score}";

        ReceiveSelectedTopic();
        GenerateQuestions();
        StartCoroutine(StartTimerCoroutine());
        InitializePlayerData();
    }

    IEnumerator StartTimerCoroutine()
    {
        int timer = this.timer;
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

        string uniqueKey = $"Question {currentQuestionIndex + 1}_{roundNumber}";

        // check if question is an image
        if (questionData.questions[currentQuestionIndex].questionText == "")
            answeredQuestions.Add(
                uniqueKey,
                new QuestionData
                {
                    question = $"An image of {questionData.questions[currentQuestionIndex].questionImage.name}",
                    correct = true
                }
            );
        else
            answeredQuestions.Add(
                uniqueKey,
                new QuestionData
                {
                    question = questionData.questions[currentQuestionIndex].questionText,
                    correct = true
                }
            );


        ChangeScoreList(playerName, score);

        AudioManager.PlaySound(correctClip);
        markText.text = "Correct!";
        StartCoroutine(ClearMarkText());
        UnfreezePlayer();
        quizPanel.SetActive(false);

        RemoveQuestion(currentQuestionIndex);

        // Reset the questions if there are no more questions
        if (questionData.questions.Length == 0)
        {
            Debug.Log("No more questions. Resetting questions...");
            roundNumber++;
            questionData = LoadQuestionData(topic, seed, total);
        }

        Debug.Log($"{topic} question count: {questionData.questions.Length}");

        GenerateQuestions();
    }

    public override void AnswerWrong()
    {
        string uniqueKey = $"Question {currentQuestionIndex + 1}_{roundNumber}";

        // check if question is an image
        if (questionData.questions[currentQuestionIndex].questionText == "")
            answeredQuestions.Add(
                uniqueKey,
                new QuestionData
                {
                    question = $"An image of {questionData.questions[currentQuestionIndex].questionImage.name}",
                    correct = false
                }
            );
        else
            answeredQuestions.Add(
                uniqueKey,
                new QuestionData
                {
                    question = questionData.questions[currentQuestionIndex].questionText,
                    correct = false
                }
            );


        AudioManager.PlaySound(wrongClip);
        markText.text = "Wrong!";
        StartCoroutine(ClearMarkText());
        UnfreezePlayer();
        quizPanel.SetActive(false);

        RemoveQuestion(currentQuestionIndex);

        // Reset the questions if there are no more questions
        if (questionData.questions.Length == 0)
        {
            Debug.Log("No more questions. Resetting questions...");
            roundNumber++;
            questionData = LoadQuestionData(topic, seed, total);
        }

        Debug.Log($"{topic} question count: {questionData.questions.Length}");

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
        quizPanel.GetComponent<UnityEngine.UI.Image>().enabled = false;
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
        SaveManager.player.SaveActivity(
            true,
            topic,
            $"{correct}/{answeredQuestions.Count}",
            answeredQuestions,
            Regex.Replace(SceneManager.GetActiveScene().name, "(?<=\\p{Ll})(?=\\p{Lu})", " "),
            PhotonNetwork.PlayerList.Select(x => x.NickName).ToArray()
        );

        AudioSource.Stop();
        StartCoroutine(UpdateScores());
    }

    // Handle player place
    IEnumerator UpdateScores()
    {
        // Show the ranking in the text
        AudioManager.PlaySound(roundClip);
        questionText.text = string.Empty;
        foreach (var entry in playerData) questionText.text += $"{entry.Key}: {entry.Value}\n";
        yield return new WaitForSeconds(5.0f);
        quizPanel.SetActive(false);

        // Notify increase
        SaveManager.player.IncreaseStat(topic, score);
        NotifyIncrease();
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
