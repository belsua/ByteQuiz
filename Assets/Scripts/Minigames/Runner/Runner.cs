using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData
{
    public string NickName { get; set; }
    public int Score { get; set; }
    public bool IsFinished { get; set; }
}

public class Runner : Minigame 
{
    public class QuestionData : BaseQuestionData { public int attempts { get; set; } }

    [SerializeField] TMP_Text scoreText, standingsText;
    [SerializeField] GameObject standingsPanel, scorePanel;
    [SerializeField] Transform teleportLocation;
    [SerializeField] Settings settings;

    private Dictionary<int, PlayerData> playerData = new Dictionary<int, PlayerData>();
    Dictionary<string, QuestionData> answeredQuestions = new(); // <question number, question data>

    public GameObject currentObject;
    int attempts = 0;

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
    }

    public void ChangeUI_RPC() 
    {
        photonView.RPC("UpdateUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) 
    {
        playerData.Remove(otherPlayer.ActorNumber);
        ChangeUI_RPC();
    }

    #region Quiz Functions

    public override void AnswerCorrect() 
    {
        AudioManager.PlaySound(correctClip);
        score += 100;

        if (questionData.questions[currentQuestionIndex].questionText == "") 
            answeredQuestions.Add(
                $"Question {currentQuestionIndex + 1}",
                new QuestionData
                {
                    question = $"An image of {questionData.questions[currentQuestionIndex].questionImage.name}",
                    attempts = attempts
                }
            );
        else 
            answeredQuestions.Add(
                $"Question {currentQuestionIndex + 1}",
                new QuestionData
                {
                    question = questionData.questions[currentQuestionIndex].questionText,
                    attempts = attempts
                }
            );

        attempts = 0;
        ChangeScoreList(PhotonNetwork.LocalPlayer.ActorNumber, score);
        ChangeUI_RPC();
        RemoveQuestion(currentQuestionIndex);
        Destroy(currentObject);
        GenerateQuestions();
        ToggleQuiz(false);
    }

    public override void AnswerWrong() 
    {
        AudioManager.PlaySound(wrongClip);
        score -= 20;
        ChangeScoreList(PhotonNetwork.LocalPlayer.ActorNumber, score);
        attempts += 1;
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

        var sortedPlayerData = playerData.OrderByDescending(x => x.Value.Score).ToList();

        int place = 1;
        int previousScore = int.MinValue;
        int samePlaceCount = 0;

        foreach (var entry in sortedPlayerData)
        {
            if (entry.Value.Score == previousScore)
            {
                samePlaceCount++;
            }
            else
            {
                place += samePlaceCount;
                samePlaceCount = 1;
                previousScore = entry.Value.Score;
            }

            if (entry.Key == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                int additionalScore = 0;

                additionalScore = place switch
                {
                    1 => 100,
                    2 => 75,
                    3 => 50,
                    _ => 25,
                };

                score += additionalScore;
                ChangeScoreList(PhotonNetwork.LocalPlayer.ActorNumber, score);
                break;
            }
        }

        other.transform.position = teleportLocation.position;

        playerData[PhotonNetwork.LocalPlayer.ActorNumber].IsFinished = true;

        ChangeFinishPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
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
        if (playerData.Values.All(x => x.IsFinished)) StartCoroutine(EndGameCoroutine());
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
        SaveManager.player.SaveActivity(
            true, // This is multiplayer mode
            topic, // The topic chosen by the host from the dropdown in the room
            $"{CalculateTotalScore(answeredQuestions)}/{total}", // The score of the player
            answeredQuestions, // The player's answers
            Regex.Replace(SceneManager.GetActiveScene().name, "(?<=\\p{Ll})(?=\\p{Lu})", " "), // The scene name
            PhotonNetwork.PlayerList.Select(x => x.NickName).ToArray() // The player names
        );

        standingsText.text = string.Empty;
        foreach (var entry in playerData) standingsText.text += $"{entry.Value.NickName}: {entry.Value.Score}\n";
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
        SaveManager.player.IncreaseStat(topic, 0.25f / CalculateTotalScore(answeredQuestions));
        settings.UpdatePlayerInterface();
        NotifyIncrease();
    }

    #endregion

    #region Game Variables

    public override void InitializePlayerData()
    {
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        foreach (Photon.Realtime.Player player in players)
        {
            playerData.Add(player.ActorNumber, new PlayerData
            {
                NickName = player.NickName,
                Score = 0,
                IsFinished = false
            });
        }
    }

    [PunRPC]
    public void UpdateScoreList(int actorNumber, int score)
    {
        playerData[actorNumber].Score = score;
    }

    public void ChangeScoreList(int actorNumber, int score) 
    {
        photonView.RPC("UpdateScoreList", RpcTarget.All, actorNumber, score);
    }

    [PunRPC]
    public void UpdateFinishPlayer(int actorNumber)
    {
        playerData[actorNumber].IsFinished = true;
    }

    public void ChangeFinishPlayer(int actorNumber)
    {
        photonView.RPC("UpdateFinishPlayer", RpcTarget.All, actorNumber);
    }

    [ContextMenu("Teleport Player")]
    public void TeleportPlayer()
    {
        GameObject.FindGameObjectWithTag("Player").transform.position = new Vector2(GameObject.FindGameObjectWithTag("Player").transform.position.x, 155f);
    }

    public float CalculateTotalScore(Dictionary<string, QuestionData> scores)
    {
        float totalScore = 0;
        foreach (var score in scores.Values) totalScore += CalculateScore(score.attempts);
        return totalScore;
    }

    private float CalculateScore(int attempts)
    {
        return attempts switch
        {
            0 => 1.0f,// 0 attempts
            1 => 0.57f,// 1 attempt
            2 => 0.50f,// 2 attempts
            3 => 0.25f,// 3 attempts
            _ => 0.0f,// More than 3 attempts
        };
    }

    #endregion
}
