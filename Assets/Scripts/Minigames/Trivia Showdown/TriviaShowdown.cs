using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class TriviaShowdown : Minigame
{
    //internal static GameObject selectedObject;

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

    public override void SpawnPlayers(int order = 0)
    {
        base.SpawnPlayers(1);
    }

    public override void StartGame()
    {
        buttons.SetActive(true);
        questionImage.gameObject.SetActive(true);

        AudioSource.Play();
        ReceiveSelectedTopic();
        GenerateQuestions();

        //if (PhotonNetwork.IsMasterClient) photonView.RPC("ShowQuestion", RpcTarget.All);
        StartCoroutine(ShowQuestionCoroutine());

        // Initialize player place
    }

    #region Game Loop

    //[PunRPC]
    //public void ShowQuestion()
    //{
    //    StartCoroutine(ShowQuestionCoroutine());
    //}

    IEnumerator ShowQuestionCoroutine()
    {
        // End the game if no questions left
        if (questionData.questions.Length == 0)
        {
            if (PhotonNetwork.IsMasterClient) StartCoroutine(EndGameCoroutine());
            yield break;
        }
        else
        {
            float time = timer;
            while (time >= 0)
            {
                quizPanel.SetActive(true);
                timerText.text = $"Time: {time}";
                yield return new WaitForSeconds(1.0f);
                time--;
            }

            // Check the answer
            //Answers selectedObject = EventSystem.current.currentSelectedGameObject.GetComponent<Answers>();
            //selectedObject.Answer();

            // Handle the score


            // Show the next question
            RemoveQuestion(currentQuestionIndex);
            Debug.Log($"Remaining questions: {questionData.questions.Length}");
            GenerateQuestions();

            StartCoroutine(ShowQuestionCoroutine());
        }
    }

    public override void AnswerCorrect()
    {
        //Debug.Log("AnswerCorrect()");
        AudioManager.PlaySound(correctClip);
        markText.text = "Correct!";

        // Save the score

        // Handle UI update
        StartCoroutine(ClearMarkText());
    }

    public override void AnswerWrong()
    {
        //Debug.Log("AnswerWrong()");
        AudioManager.PlaySound(wrongClip);
        markText.text = "Wrong!";

        // Save the score

        // Handle UI update
        StartCoroutine(ClearMarkText());
    }

    IEnumerator ClearMarkText()
    {
        yield return new WaitForSeconds(2.5f);
        markText.text = string.Empty;
    }

    IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(3);
        RPCEndGame();
    }

    public void RPCEndGame()
    {
        photonView.RPC("EndGame", RpcTarget.All);
    }

    [PunRPC]
    public override void EndGame()
    {
        // Debug.Log("EndGame()");
        AudioSource.Stop();

        if (questionImage.isActiveAndEnabled)
        {
            questionImage.gameObject.SetActive(false);
            questionText.gameObject.SetActive(true);
        }
        else
        {
            questionText.gameObject.SetActive(true);
        }

        buttons.SetActive(false);
        questionText.text = "Finished!";

        AudioManager.PlaySound(finishClip);

        StartCoroutine(DisplayScores());

        // SaveManager.selectedPlayer.IncreaseStat(topic, score / 200);
    }

    // Handle player place
    IEnumerator DisplayScores()
    {
        yield return new WaitForSeconds(3.0f);
        // Show the ranking in the text


        // Increase stats
        StartCoroutine(NotifyIncrease());
    }

    IEnumerator NotifyIncrease()
    {
        yield return new WaitForSeconds(3.0f);

        // Return to lobby
        StartCoroutine(LoadLobby());
    }

    IEnumerator LoadLobby()
    {
        yield return new WaitForSeconds(3.0f);
        
        PhotonNetwork.LoadLevel("Room");
    }

    #endregion

}
