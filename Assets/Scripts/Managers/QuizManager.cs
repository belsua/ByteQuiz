using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class QuizManager : MonoBehaviourPunCallbacks
{
    public QuizData quizData;
    public TextMeshProUGUI questionText;
    public GameObject[] options;
    public GameObject quizPanel;
    public GameObject controls;
    public GameObject currentObject;
    public string selectedTopic;

    private int currentQuestionIndex;

    private void Start()
    {
        ReceiveSelectedTopic();
        GenerateQuestions();
    }

    private void ReceiveSelectedTopic()
    {
        if (PhotonNetwork.InRoom)
        {
            Player player = PhotonNetwork.LocalPlayer;
            if (player.CustomProperties.ContainsKey("selectedTopic"))
            {
                selectedTopic = (string)player.CustomProperties["selectedTopic"];
                QuizData quizData = LoadQuizData(selectedTopic);
                if (quizData != null)
                {
                    this.quizData = quizData;
                }
                else
                {
                    Debug.LogError($"Failed to load quiz data for the selected topic: {selectedTopic}");
                }
            }
            else
            {
                Debug.LogError("Selected topic not found in CustomProperties.");
            }
        }
    }

    QuizData LoadQuizData(string topic)
    {
        QuizData originalData = Resources.Load<QuizData>(topic);
        if (originalData == null)
        {
            Debug.LogError($"Failed to load QuizData for topic: {topic}");
            return null;
        }

        QuizData copyData = Instantiate(originalData);
        return copyData;
    }

    public void ToggleQuiz(bool state)
    {
        quizPanel.SetActive(state);
        controls.SetActive(!state);
    }

    public void AnswerCorrect() 
    {
        if (quizData != null && currentQuestionIndex >= 0 && currentQuestionIndex < quizData.questions.Length)
        {
            RemoveQuestion(currentQuestionIndex);

            if (currentObject != null)
                PhotonNetwork.Destroy(currentObject);

            GenerateQuestions();
            ToggleQuiz(false);
        }
        else
        {
            Debug.LogError("No QuizData or invalid question index.");
        }
    }

    void SetAnswers()
    {
        if (quizData != null && currentQuestionIndex >= 0 && currentQuestionIndex < quizData.questions.Length)
        {
            for (int i = 0; i < options.Length; i++)
            {
                options[i].GetComponent<Answers>().isCorrect = false;
                options[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = quizData.questions[currentQuestionIndex].answers[i];

                if (quizData.questions[currentQuestionIndex].correctAnswerIndex == i)
                    options[i].GetComponent<Answers>().isCorrect = true;
            }
        }
        else
        {
            Debug.LogError("No QuizData or invalid question index.");
        }
    }

    void GenerateQuestions()
    {
        if (quizData != null && quizData.questions.Length > 0)
        {
            currentQuestionIndex = Random.Range(0, quizData.questions.Length);
            questionText.text = quizData.questions[currentQuestionIndex].question;

            SetAnswers();
        }
        else
        {
            Debug.Log("No questions available in QuizData.");
        }
    }

    void RemoveQuestion(int index)
    {
        for (int i = index + 1; i < quizData.questions.Length; i++)
            quizData.questions[i - 1] = quizData.questions[i];

        System.Array.Resize(ref quizData.questions, quizData.questions.Length - 1);
    }
}
