using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAnimation : MonoBehaviour
{
    public GameObject Object;
    [SerializeField] RectTransform targetTransform;
    [SerializeField] RectTransform targetOriginalTransform;
    [SerializeField] float duration;
    [SerializeField] LeanTweenType easeType;

    public PlayerController playerController; // Reference to the PlayerController script
    private bool isButtonOffScreen = false; // To track the button state

    private float screenWidth; // Variable to store the screen width
    private float movementOffset; // The calculated offset for moving buttons

    void Start()
    {
        // Get screen width at the start
        screenWidth = Screen.width;

        // Set movementOffset to be relative to the screen width (e.g., 30% of screen width)
        movementOffset = screenWidth * 0.1f;
    }

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
        // Move button off-screen using relative offset
        LeanTween.moveX(gameObject, targetTransform.position.x, duration).setEase(easeType); 
    }

    void MoveIn()
    {
        // Move button back to its original position
        LeanTween.moveX(gameObject, targetOriginalTransform.position.x, duration).setEase(easeType);
    }
}
