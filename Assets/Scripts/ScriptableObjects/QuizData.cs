using UnityEngine;

[CreateAssetMenu(fileName = "New QuizData", menuName = "Quiz/Quiz Data")]
public class QuizData : ScriptableObject
{
    public Question[] questions;
}
