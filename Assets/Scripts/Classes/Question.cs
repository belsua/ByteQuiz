[System.Serializable]
public class Question
{
    public string question;
    public string[] answers;
    public int correctAnswerIndex;

    public bool IsCorrect(int answerIndex) => answerIndex == correctAnswerIndex;
}