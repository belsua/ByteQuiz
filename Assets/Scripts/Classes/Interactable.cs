using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    Runner runner;
    Button interactButton;

    protected virtual void Start()
    {
        runner = FindAnyObjectByType<Runner>();
        interactButton = GameObject.Find("InteractButton").GetComponent<Button>();
        interactButton.onClick.AddListener(() => runner.ToggleQuiz(true));
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            interactButton.interactable = true;

        // Save the collided object in the variable
            TeleportLocation.collidedObject = gameObject;
            Debug.Log($"Collided with: {gameObject.name}");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        interactButton.interactable = false;
    }
}
