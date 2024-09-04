using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;
using System.IO;

public class TriviaShowdown : Minigame
{
    Dictionary<string, int> playerData = new(); // <player, score>

    [Range(1, 30)]
    [SerializeField] int timer = 20;
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
        correct++;
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
        questionImage.gameObject.SetActive(false);
        questionText.text = "Finished!";
        AudioManager.PlaySound(finishClip);
        yield return new WaitForSeconds(5);
        RPCEndGame();
    }

    public void RPCEndGame()
    {
        photonView.RPC("EndMinigame", RpcTarget.All);
    }

    [PunRPC]
    public override void EndMinigame()
    {
        // Increase stats
        SaveManager.player.IncreaseStat(topic, (float)score / 100f);
        SaveManager.SavePlayer(SaveManager.player.slot);

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
        GameObject.Find("Barrier").SetActive(false);
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
        var sorted = playerData.OrderByDescending(x => x.Value).ToList();
        int currentIndex = sorted.FindIndex(entry => entry.Key == playerName);
        if (currentIndex != -1)
        {
            int rank = currentIndex + 1;
            string suffix = GetRankSuffix(rank);
            placeText.text = $"Place: {rank}{suffix}";
        }
    }

    private string GetRankSuffix(int rank)
    {
        if (rank % 100 >= 11 && rank % 100 <= 13) return "th";

        switch (rank % 10)
        {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    }

    #endregion
}
