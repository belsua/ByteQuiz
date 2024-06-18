using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnswer : MonoBehaviour
{
    public int answerIndex;
    internal object onClick;

    public void SetAnswerIndex(int index)
    {
        answerIndex = index;
    }
}
