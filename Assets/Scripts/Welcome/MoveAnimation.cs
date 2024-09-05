using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAnimation : MonoBehaviour
{
    public GameObject Object;
    [SerializeField] RectTransform targetTransform;
    [SerializeField] float duration;
    [SerializeField] LeanTweenType easeType;

    public PlayerController playerController; // Reference to the PlayerController script
    private bool isButtonOffScreen = false; // To track the button state

    void Update()
    {
        // Check if the player is moving
        if (playerController.Movement.sqrMagnitude > 0) // Character is moving
        {
            if (!isButtonOffScreen) // If the button is not already off the screen
            {
                MoveOut(); // Move button off-screen
                isButtonOffScreen = true;
            }
        }
        else // Character has stopped moving
        {
            if (isButtonOffScreen) // If the button is off the screen
            {
                MoveIn(); // Move button back to original position
                isButtonOffScreen = false;
            }
        }
    }

    void MoveOut()
    {
        // Move button out of the screen (modify the targetTransform as needed for off-screen position)
        LeanTween.moveX(gameObject, targetTransform.position.x, duration).setEase(easeType); // Example: Move 500 units to the right
    }

    void MoveIn()
    {
        // Move button back to its original position
        LeanTween.moveX(gameObject, targetTransform.position.x + 300, duration).setEase(easeType);
    }
}
