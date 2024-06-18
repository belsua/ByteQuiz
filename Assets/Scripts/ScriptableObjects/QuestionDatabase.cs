using UnityEngine;

[CreateAssetMenu(fileName = "New Question Database", menuName = "Quiz/Question Database")]
public class QuestionDatabase : ScriptableObject
{
    public PlayQuestion[] questions;
}
