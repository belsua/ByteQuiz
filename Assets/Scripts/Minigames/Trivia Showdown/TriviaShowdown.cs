using UnityEngine;
using TMPro;
using System.Collections;

public class TriviaShowdown : Minigame
{
    [Range(1, 10)]
    [SerializeField] int timer;
    TMP_Text timerText;

    protected override void Awake()
    {
        base.Awake();
        timerText = GameObject.Find("TimerText").GetComponent<TMP_Text>();
        timerText.text = string.Empty;
    }

    public override void StartGame()
    {
        AudioSource.Play();
        ReceiveSelectedTopic();
        GenerateQuestions();

        StartCoroutine(ShowQuestion());
    }

    public override void EndGame()
    {
        
    }

    protected override void SetAnswers()
    {

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

        // Save the selected answer

        // Show the next question
    }

    #endregion

}
