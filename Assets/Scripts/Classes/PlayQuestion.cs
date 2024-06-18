// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayQuestion
// {
//     public class Question
//     {
//         public string questionText;
//         public string[] answers; // For multiple choice
//         public int correctAnswerIndex; // Index of the correct answer
//     }
// }

// [System.Serializable]
// public class PlayQuestion
// {
//     public string question;
//     public string[] answers;
//     public int correctAnswerIndex;
//     public string questionText;
//     public string playquestion;

//     public bool IsCorrect(int answerIndex) => answerIndex == correctAnswerIndex;
// }

using UnityEngine;

[System.Serializable]
public class PlayQuestion
{
    public string questionText; // Single field for question text
    public Sprite questionImage; //ADDED: New field for question image
    public string[] answers;
    public int correctAnswerIndex;

    public bool IsCorrect(int answerIndex) => answerIndex == correctAnswerIndex;
}
