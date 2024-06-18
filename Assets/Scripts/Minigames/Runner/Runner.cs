using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Runner : Minigame
{
    [SerializeField] AudioSource AudioSource;
    [SerializeField] AudioClip correctClip, wrongClip;
    [SerializeField] TMP_Text questionText, scoreText, standingsText;
    [SerializeField] GameObject[] options;
    [SerializeField] GameObject quizPanel, controls, standingsPanel;

    public GameObject currentObject;
    QuizData quizData;
    int currentQuestionIndex;

    Dictionary<string, int> scoreDict = new();
    int score;
    bool isGameActive = false;

    //private void Update()
    //{
    //    if (scoreDict.Count == PhotonNetwork.CurrentRoom.PlayerCount) ShowStandings();
    //}

    public override void StartGame()
    {
        scoreText.transform.parent.gameObject.SetActive(true);
        AudioSource.Play();
        ReceiveSelectedTopic();
        GenerateQuestions();
        isGameActive = true;
        score = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        scoreText.text = $"Score: {score}";
    }

    public override void EndGame()
    {
        throw new NotImplementedException();
    }

    //public override void ScorePoints()
    //{
    //    throw new System.NotImplementedException();
    //}

    #region Question Initiation

    void ReceiveSelectedTopic()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("selectedTopic"))
        {
            string topic = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedTopic"];
            quizData = LoadQuizData(topic);
        }
        else
        {
            Debug.LogError("Selected topic not found in CustomProperties.");
        }
    }

    QuizData LoadQuizData(string topic)
    {
        QuizData originalData = Resources.Load<QuizData>($"Questions/{topic}");

        if (originalData == null)
        {
            Debug.LogError($"Failed to load QuizData for topic: {topic}");
            return null;
        }

        return Instantiate(originalData);
    }

    #endregion

    #region Quiz Functions

    void GenerateQuestions()
    {
        for (int i = 0; i < quizData.questions.Length; i++)
        {
            currentQuestionIndex = i;
            questionText.text = quizData.questions[currentQuestionIndex].question;
            SetAnswers();
        }
    }

    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<Answers>().isCorrect = false;
            options[i].GetComponentInChildren<TMP_Text>().text = quizData.questions[currentQuestionIndex].answers[i];

            if (quizData.questions[currentQuestionIndex].correctAnswerIndex == i)
                options[i].GetComponent<Answers>().isCorrect = true;
        }        
    }

    internal void AnswerCorrect()
    {
        AudioManager.PlaySound(correctClip);
        score = Mathf.Clamp(score + 100, 0, 1000);
        UpdateUI();
        RemoveQuestion(currentQuestionIndex);
        Destroy(currentObject);
        GenerateQuestions();
        ToggleQuiz(false);
    }

    internal void AnswerWrong()
    {
        AudioManager.PlaySound(wrongClip);
        score = Mathf.Clamp(score - 20, 0, 1000);
        UpdateUI();
    }

    public void ToggleQuiz(bool state)
    {
        quizPanel.SetActive(state);
        controls.SetActive(!state);
    }

    void RemoveQuestion(int index)
    {
        for (int i = index + 1; i < quizData.questions.Length; i++)
            quizData.questions[i - 1] = quizData.questions[i];

        Array.Resize(ref quizData.questions, quizData.questions.Length - 1);
    }

    //void ShowStandings()
    //{
    //    scoreDict.Add(PhotonNetwork.LocalPlayer.NickName, score);
    //    standingsPanel.SetActive(true);
    //    standingsText.text = "Standings: ";

    //    List<KeyValuePair<string, int>> kvpList = new List<KeyValuePair<string, int>>(scoreDict);
    //    for (int i = 0; i < kvpList.Count; i++)
    //    {
    //        var kvp = kvpList[i];
    //        standingsText.text += $"{i + 1}. {kvp.Key}: {kvp.Value}\n";
    //    }

    //    Invoke("ReturnToLobby", 5f);
    //}

    //void ReturnToLobby()
    //{
    //    PhotonNetwork.LoadLevel("LobbyScene");
    //}

    #endregion

    #region Finish Line Functions

    [SerializeField] Transform teleportLocation;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (other.CompareTag("Player"))
        //{
        //    Teleport(other.gameObject);
        //}
        PhotonNetwork.LoadLevel("Room");
    }

    //private void Teleport(GameObject player)
    //{
    //    player.transform.position = teleportLocation.position;
        
    //}

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon with cause: " + cause);
        SceneManager.LoadScene("Lobby");
    }

    #endregion
}
