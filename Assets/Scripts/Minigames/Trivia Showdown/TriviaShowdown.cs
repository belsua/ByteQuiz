using UnityEngine;
using TMPro;
using System.Collections;

public class TriviaShowdown : Minigame
{
    [Range(1, 10)]
    [SerializeField] int timer;

    protected override void Awake()
    {
        base.Awake();
        GameObject.Find("TimerText").GetComponent<TMP_Text>().text = string.Empty;
        //InitializeQuestions();
    }

    public override void StartGame()
    {
        AudioSource.Play();
        ReceiveSelectedTopic();
        GenerateQuestions();
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
        while (time > 0)
        {
            messageText.text = $"Time: {time}";
            yield return new WaitForSeconds(1.0f);
            time--;
        }

        // Save the selected answer

        // Show the next question
    }

    #endregion

    //private void InitializeQuestions()
    //{
    //    #if UNITY_EDITOR
    //        quizData = LoadQuizData("EOCS");
    //    #else
    //    if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("selectedTopic")) {
    //        Debug.Log($"Topic: {topic}");
    //        topic = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedTopic"];
    //        quizData = LoadQuizData(topic);
    //    }
    //    else {
    //        Debug.LogError("Selected topic not found in CustomProperties.");
    //    }
    //    #endif
    //}
}
