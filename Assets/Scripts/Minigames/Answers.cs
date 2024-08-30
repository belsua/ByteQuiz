using UnityEngine;

public class Answers : MonoBehaviour
{
    public bool isCorrect = false;
    public MonoBehaviour minigame;

    private IMinigame minigameLogic;

    private void Awake()
    {
        minigameLogic = minigame as IMinigame;

        if (minigameLogic == null)
        {
            Debug.LogError("Assigned minigame does not implement IMinigame interface.");
        }
    }

    public void Answer()
    {
        if (minigameLogic == null) return;

        if (isCorrect) minigameLogic.AnswerCorrect();
        else minigameLogic.AnswerWrong();
    }
}
