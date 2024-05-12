using UnityEngine;

public class Answers : MonoBehaviour
{
    public bool isCorrect = false;
    public QuizManager quizManager;

    public void Answer()
    {
        if (isCorrect)
        {
            Debug.Log($"Correct!");
            quizManager.AnswerCorrect();
        }
        else
        {
            Debug.Log($"Wrong!");
        }
    }
}
