using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayManager : MonoBehaviour
{
    [Header("Question Database")]
    public QuestionDatabase[] questionDatabases; // Array of question databases for each topic
    public List<PlayQuestion> currentQuestions;
    public TextMeshProUGUI questionText;
    public Image questionImage; //ADDED Add a reference to an Image component
    [Header("Game Loop")]
    public int score;
    [Header("Components")]
    public CribManager cribManager;
    public Button[] answerButtons;
    public TextMeshProUGUI feedbackText;

    private int currentQuestionIndex;
    private int topicIndex;
    private const int MaxQuestionsPerQuiz = 29; //LIMIT QUIZ 0-29 = 30 questions

    private void Start()
    {
        foreach (Button button in answerButtons)
        {
            button.onClick.AddListener(() => OnAnswerSelected(button));
        }
    }

    public void SelectTopic(int topicIndex)
    {
        this.topicIndex = topicIndex;

        currentQuestions = new List<PlayQuestion>(questionDatabases[topicIndex].questions);
        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.Log("No questions available for the selected topic or failed to load questions.");
            return;
        }
        Shuffle(currentQuestions);

        //ADDED FOR QUIZ LIMIT
        if (currentQuestions.Count > MaxQuestionsPerQuiz)
        {
            currentQuestions = currentQuestions.GetRange(0, MaxQuestionsPerQuiz);
        }
        //ADDED FOR QUIZ LIMIT

        currentQuestionIndex = 0;
        score = 0;
        LoadQuestion();
    }

    private void Shuffle(List<PlayQuestion> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            PlayQuestion temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void LoadQuestion()
    {
        if (currentQuestionIndex < currentQuestions.Count)
        {
            PlayQuestion question = currentQuestions[currentQuestionIndex];
            // Debug.Log("Loading question: " + question.questionText);
            // Debug.Log($"Loading question: '{question.questionText}' with {question.answers.Length} answers.");
            
            questionText.text = question.questionText;
            Debug.Log("Loading question: " + question.questionText);

            //ADDED
            // Display the question image if it exists
            if (question.questionImage != null)
            {
                questionImage.sprite = question.questionImage;
                questionImage.gameObject.SetActive(true);
            }
            else
            {
                questionImage.gameObject.SetActive(false);
            }


            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < question.answers.Length)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.answers[i];
                    answerButtons[i].GetComponent<PlayAnswer>().SetAnswerIndex(i);
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            EndQuiz();
        }
    }

    private void OnAnswerSelected(Button button)
    {
        int selectedAnswerIndex = button.GetComponent<PlayAnswer>().answerIndex;
        PlayQuestion question = currentQuestions[currentQuestionIndex];

        if (selectedAnswerIndex == question.correctAnswerIndex)
        {
            feedbackText.text = "Correct!";
            score++;
        }
        else
        {
            feedbackText.text = "Wrong!";
        }

        currentQuestionIndex++;
        LoadQuestion();
    }

    private string GetTopic(int index)
    {
        return index switch
        {
            0 => "HOC",
            1 => "EOCS",
            2 => "NS",
            3 => "ITP",
            _ => "HOC",
        };
    }

    private void EndQuiz()
    {
        SaveManager.player.IncreaseStat(GetTopic(topicIndex), score / 300f);
        questionText.text = "Quiz Over! Your score: " + score;
        cribManager.UpdatePlayerInterface();
        cribManager.ShowMessage($"Your {GetTopic(topicIndex)} stat increased!");

        foreach (var button in answerButtons)
        {
            button.gameObject.SetActive(false);
        }
        feedbackText.text = "";

        //ADDED
        questionImage.gameObject.SetActive(false); // Hide the image at the end
    }
}
