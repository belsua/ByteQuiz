using UnityEngine;
using UnityEngine.UI;

public class Interactable : Collidable
{
    QuizManager quizManager;
    Button interactButton;

    protected virtual void Start()
    {
        quizManager = FindAnyObjectByType<QuizManager>();
        interactButton = GameObject.Find("InteractButton").GetComponent<Button>();

        interactButton.onClick.AddListener(() => quizManager.ToggleQuiz(true));
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            interactButton.interactable = true;

        // Save the collided object in the variable
        quizManager.currentObject = gameObject;
        Debug.Log($"Collided with: {gameObject.name}");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        interactButton.interactable = false;
    }
}
