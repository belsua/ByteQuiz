using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupsAnimationX : MonoBehaviour
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
    private bool collided = false;

    void Start()
    {
        screenWidth = Screen.width;
        movementOffset = screenWidth * 0.1f;
    }

    // Use OnTriggerEnter2D to detect when the player enters the trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MoveIn();
            Debug.Log("Entered");
            collided = true;
        }
    }

    // Use OnTriggerExit2D to detect when the player exits the trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MoveOut();
            Debug.Log("Out");
            collided = false;
        }
    }

    void MoveOut()
    {
        // Move Object off-screen using relative offset
        LeanTween.moveX(Object, targetTransform.position.x, duration).setEase(easeType); 
    }

    void MoveIn()
    {
        // Move Object back to its original position
        LeanTween.moveX(Object, targetOriginalTransform.position.x, duration).setEase(easeType);
    }
}
