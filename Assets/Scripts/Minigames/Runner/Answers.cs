using UnityEngine;

public class Answers : MonoBehaviour
{
    public bool isCorrect = false;
    public Runner runner;

    public void Answer()
    {
        if (isCorrect) runner.AnswerCorrect();
        else runner.AnswerWrong();
    }
}
