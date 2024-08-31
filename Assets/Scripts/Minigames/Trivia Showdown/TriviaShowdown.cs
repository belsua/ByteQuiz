using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class TriviaShowdown : Minigame
{
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
        questionImage.gameObject.SetActive(false);
    }

    public override void SpawnPlayers(int order = 0)
    {
        base.SpawnPlayers(1);
    }

    public override void StartGame()
    {
        questionImage.gameObject.SetActive(true);

        AudioSource.Play();
        ReceiveSelectedTopic();
        GenerateQuestions();

        StartCoroutine(ShowQuestion());

        // Initialize player place
    }

    #region Game Loop

    IEnumerator ShowQuestion()
    {
        float time = timer;
        while (time >= 0)
        {
            quizPanel.SetActive(true);
            timerText.text = $"Time: {time}";
            yield return new WaitForSeconds(1.0f);
            time--;
        }

        // End the game if no questions left
        if (questionData.questions.Length == 0) { EndGame(); yield break; }

        // Check the answer
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject == null)
        {
            foreach (GameObject option in options)
            {
                if (!option.GetComponent<Answers>().isCorrect)
                {
                    selectedObject = option;
                    break;
                }
            }
        }

        // Handle the score
        selectedObject.GetComponent<Answers>().Answer();

        // Show the next question
        RemoveQuestion(currentQuestionIndex);
        Debug.Log($"Remaining questions: {questionData.questions.Length}");
        GenerateQuestions();


        StartCoroutine(ShowQuestion());
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

        GameObject.Find("Buttons").SetActive(false);
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
