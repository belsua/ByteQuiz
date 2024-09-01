using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

public class TriviaShowdown : Minigame
{
    Dictionary<string, int> playerData = new(); // <player, score>

    [Range(1, 10)]
    [SerializeField] int timer;
    TMP_Text timerText, placeText, markText;

    protected override void Awake()
    {
        base.Awake();
        timerText = GameObject.Find("TimerText").GetComponent<TMP_Text>();
        timerText.text = string.Empty;

        placeText = GameObject.Find("PlaceText").GetComponent<TMP_Text>();
        placeText.text = string.Empty;

        markText = GameObject.Find("MarkText").GetComponent<TMP_Text>();
        markText.text = string.Empty;
    }

    protected override void Start()
    {
        base.Start();
        quizPanel.SetActive(true);
        buttons.SetActive(false);
        questionImage.gameObject.SetActive(false);
    }

    #region Game Loop

    public override void SpawnPlayers(int order = 0)
    {
        base.SpawnPlayers(1);
    }

    public override void InitializePlayerData()
    {
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        foreach (Photon.Realtime.Player player in players)
            playerData.Add(player.NickName, 0);
    }

    public override void StartMinigame()
    {
        // User Interface
        buttons.SetActive(true);
        questionImage.gameObject.SetActive(true);
        AudioSource.Play();

        // Start quiz questions
        score = 0;
        ReceiveSelectedTopic();
        GenerateQuestions();
        StartCoroutine(ShowQuestionCoroutine());

        // Initialize player place
        InitializePlayerData();
        ChangeUI_RPC();
    }

    IEnumerator ShowQuestionCoroutine()
    {
        // Continue quiz if still has questions left, else end game
        if (questionData.questions.Length == 0)
        {
            buttons.SetActive(false);

            if (PhotonNetwork.IsMasterClient) StartCoroutine(EndGameCoroutine());
            yield break;
        }

        float time = timer;
        while (time >= 0)
        {
            quizPanel.SetActive(true);
            timerText.text = $"Time: {time}";
            yield return new WaitForSeconds(1.0f);
            time--;
        }

        // Update game data
        //Answers selectedAnswer;
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            foreach (GameObject option in options)
            {
                if (!option.GetComponent<Answers>().isCorrect)
                {
                    option.GetComponent<Answers>().Answer();
                    break;
                }
            }
        }
        else
        {
            EventSystem.current.currentSelectedGameObject.GetComponent<Answers>().Answer();
        }

        Debug.Log($"Score: {score}");

        // Handle next question
        RemoveQuestion(currentQuestionIndex);
        GenerateQuestions();
        StartCoroutine(ShowQuestionCoroutine());
    }

    public override void AnswerCorrect()
    {
        // Save the score
        score = Mathf.Clamp(score + 1, 0, int.MaxValue);
        ChangeScoreList(playerName, score);

        // Handle UI update
        AudioManager.PlaySound(correctClip);
        ChangeUI_RPC();
        markText.text = "Correct!";
        StartCoroutine(ClearMarkText());
    }

    public override void AnswerWrong()
    {
        // Handle UI update
        AudioManager.PlaySound(wrongClip);
        ChangeUI_RPC();
        markText.text = "Wrong!";
        StartCoroutine(ClearMarkText());
    }

    IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(3);
        questionText.text = "Finished!";
        AudioManager.PlaySound(finishClip);
        yield return new WaitForSeconds(5);
        RPCEndGame();
    }

    public void RPCEndGame()
    {
        photonView.RPC("EndGame", RpcTarget.All);
    }

    [PunRPC]
    public override void EndGame()
    {
        // Increase stats
        SaveManager.selectedPlayer.IncreaseStat(topic, score / 200);

        // Handle UI
        AudioSource.Stop();
        questionImage.gameObject.SetActive(false);
        questionText.gameObject.SetActive(true);

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

    #region User Interface 

    IEnumerator ClearMarkText()
    {
        yield return new WaitForSeconds(2.5f);
        markText.text = string.Empty;
    }

    public void ChangeUI_RPC()
    {
        photonView.RPC("UpdateUI", RpcTarget.All);
    }

    [PunRPC]
    private void UpdateUI()
    {
        // Sort players by their scores in descending order
        var sorted = playerData.OrderByDescending(x => x.Value).ToList();

        // Find the index of the current player
        int currentIndex = sorted.FindIndex(entry => entry.Key == playerName);

        // If the player is found in the sorted list
        if (currentIndex != -1)
        {
            // Determine the rank position (1-based index)
            int rank = currentIndex + 1;

            // Choose the correct suffix for the rank (e.g., "1st", "2nd", "3rd", "4th", etc.)
            string suffix = GetRankSuffix(rank);

            // Update the place text with the correct rank and suffix
            placeText.text = $"Place: {rank}{suffix}";
        }
    }

    // Method to determine the correct suffix for a rank
    private string GetRankSuffix(int rank)
    {
        if (rank % 100 >= 11 && rank % 100 <= 13)
        {
            return "th";
        }

        switch (rank % 10)
        {
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            default:
                return "th";
        }
    }

    #endregion
}
