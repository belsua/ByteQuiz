using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayManager : MonoBehaviour
{
    public class QuestionData : BaseQuestionData { }

    [Header("Question Database")]
    public QuestionDatabase[] questionDatabases; // Array of question databases for each topic
    public List<PlayQuestion> currentQuestions;
    public TextMeshProUGUI questionText;
    public Image questionImage; //ADDED Add a reference to an Image component
    [Header("Game Loop")]
    public int score;
    [Range(1, 50)]
    public int MaxQuestions = 30;
    [Header("Components")]
    public Settings settings;
    public CribManager cribManager;
    public Button[] answerButtons;
    public AudioClip correctClip;
    public AudioClip wrongClip;

    private int currentQuestionIndex;
    private int topicIndex;
    public Dictionary<string, QuestionData> answeredQuestions = new(); // <question number, question data>

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
        if (currentQuestions.Count > MaxQuestions)
        {
            currentQuestions = currentQuestions.GetRange(0, MaxQuestions);
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
            cribManager.QuickShowMessage("Correct!");
            AudioManager.PlaySound(correctClip);
            score++;

            if (currentQuestions[currentQuestionIndex].questionText == "")
                answeredQuestions.Add(
                    $"Question {currentQuestionIndex + 1}",
                    new QuestionData
                    {
                        question = $"An image of {currentQuestions[currentQuestionIndex].questionImage.name}",
                        correct = true
                    }
                );
            else
                answeredQuestions.Add(
                $"Question {currentQuestionIndex + 1}",
                    new QuestionData
                    {
                        question = currentQuestions[currentQuestionIndex].questionText,
                        correct = true
                    }
                );
        }
        else
        {
            cribManager.QuickShowMessage("Wrong!");
            AudioManager.PlaySound(wrongClip);

            if (currentQuestions[currentQuestionIndex].questionText == "")
                answeredQuestions.Add(
                    $"Question {currentQuestionIndex + 1}",
                    new QuestionData
                    {
                        question = $"An image of {currentQuestions[currentQuestionIndex].questionImage.name}",
                        correct = false
                    }
                );
            else
                answeredQuestions.Add(
                $"Question {currentQuestionIndex + 1}",
                    new QuestionData
                    {
                        question = currentQuestions[currentQuestionIndex].questionText,
                        correct = false
                    }
                );
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
        SaveManager.player.SaveActivity(
            false,
            GetTopic(topicIndex),
            $"{score}/{MaxQuestions}",
            answeredQuestions
        );

        SaveManager.player.IncreaseStat(GetTopic(topicIndex), score);
        questionText.text = "Quiz Over! Your score: " + score;
        settings.UpdatePlayerInterface();
        cribManager.UpdateButtons();
        cribManager.ShowMessage($"Your {GetTopic(topicIndex)} stat increased!");
        cribManager.MessageCloseCountdown();


        foreach (var button in answerButtons)
        {
            button.gameObject.SetActive(false);
        }

        //ADDED
        questionImage.gameObject.SetActive(false); // Hide the image at the end
    }
}
