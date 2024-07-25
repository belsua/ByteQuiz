using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Runner : Minigame {
    [SerializeField] AudioSource AudioSource;
    [SerializeField] AudioClip correctClip, wrongClip;
    [SerializeField] TMP_Text questionText, scoreText, standingsText, scoreListText;
    [SerializeField] GameObject[] options;
    [SerializeField] GameObject quizPanel, controls, standingsPanel;

    Dictionary<string, int> scoreList = new();
    bool[] isPlayerFinished = new bool[4];

    public GameObject currentObject;
    QuizData quizData;
    int currentQuestionIndex;

    int score;
    bool isGameActive = false;

    public override void StartGame() {
        scoreText.transform.parent.gameObject.SetActive(true);
        AudioSource.Play();
        ReceiveSelectedTopic();
        GenerateQuestions();
        isGameActive = true;
        score = 0;
        InitializeScoreList();
        ChangeUI();
    }

    [PunRPC]
    private void UpdateUI() {
        scoreText.text = $"Score: {score}";
        scoreListText.text = string.Empty;
        foreach (var entry in scoreList) {
            scoreListText.text += $"{entry.Key}: {entry.Value}\n";
        }
    }

    public void ChangeUI() {
        photonView.RPC("UpdateUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        scoreList.Remove(otherPlayer.NickName);
        ChangeUI();
    }

    public override void EndGame() {
        throw new NotImplementedException();
    }

    #region Question Initiation

    void ReceiveSelectedTopic() {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("selectedTopic")) {
            string topic = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedTopic"];
            quizData = LoadQuizData(topic);
        }
        else {
            Debug.LogError("Selected topic not found in CustomProperties.");
        }
    }

    QuizData LoadQuizData(string topic) {
        QuizData originalData = Resources.Load<QuizData>($"Questions/{topic}");

        if (originalData == null) {
            Debug.LogError($"Failed to load QuizData for topic: {topic}");
            return null;
        }

        return Instantiate(originalData);
    }

    #endregion

    #region Quiz Functions

    void GenerateQuestions() {
        for (int i = 0; i < quizData.questions.Length; i++) {
            currentQuestionIndex = i;
            questionText.text = quizData.questions[currentQuestionIndex].question;
            SetAnswers();
        }
    }

    void SetAnswers() {
        for (int i = 0; i < options.Length; i++) {
            options[i].GetComponent<Answers>().isCorrect = false;
            options[i].GetComponentInChildren<TMP_Text>().text = quizData.questions[currentQuestionIndex].answers[i];

            if (quizData.questions[currentQuestionIndex].correctAnswerIndex == i)
                options[i].GetComponent<Answers>().isCorrect = true;
        }
    }

    internal void AnswerCorrect() {
        AudioManager.PlaySound(correctClip);
        int oldScore = scoreList[PhotonNetwork.LocalPlayer.NickName];
        score = Mathf.Clamp(score + 100, 0, 1000);
        ChangeScoreList(PhotonNetwork.LocalPlayer.NickName, score + oldScore);
        ChangeUI();
        RemoveQuestion(currentQuestionIndex);
        Destroy(currentObject);
        GenerateQuestions();
        ToggleQuiz(false);
    }

    internal void AnswerWrong() {
        AudioManager.PlaySound(wrongClip);
        int oldScore = scoreList[PhotonNetwork.LocalPlayer.NickName];
        score = Mathf.Clamp(score - 20, 0, 1000);
        ChangeUI();
    }

    public void ToggleQuiz(bool state) {
        quizPanel.SetActive(state);
        controls.SetActive(!state);
    }

    void RemoveQuestion(int index) {
        for (int i = index + 1; i < quizData.questions.Length; i++)
            quizData.questions[i - 1] = quizData.questions[i];

        Array.Resize(ref quizData.questions, quizData.questions.Length - 1);
    }

    #endregion

    #region Finish Line Functions

    [SerializeField] Transform teleportLocation;

    private void OnTriggerEnter2D(Collider2D other) {
        other.transform.position = teleportLocation.position;
    }

    public override void OnDisconnected(DisconnectCause cause) {
        Debug.LogWarning("Disconnected from Photon with cause: " + cause);
        SceneManager.LoadScene("Lobby");
    }

    #endregion

    #region Game Variables

    public void InitializeScoreList() {
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        foreach (Photon.Realtime.Player player in players) {
            scoreList.Add(player.NickName, 0);
        }
    }

    [PunRPC]
    public void UpdateScoreList(string player, int score) {
        scoreList[player] = score;
    }

    public void ChangeScoreList(string player, int score) {
        photonView.RPC("UpdateScoreList", RpcTarget.All, player, score);
    }

    #endregion

    // TODO: Show dialog when all players are reach the finish line
    // TODO: Then update their player data
    // TODO: Bring back player to p[layer lobby
}
