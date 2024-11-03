using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupsAnimation : MonoBehaviour
{
    public GameObject Object;
    [SerializeField] RectTransform targetTransform;
    [SerializeField] RectTransform targetOriginalTransform;
    [SerializeField] float duration;
    [SerializeField] LeanTweenType easeType;

    public PlayerController playerController; // Reference to the PlayerController script

    private float screenWidth; // Variable to store the screen width
    private float movementOffset; // The calculated offset for moving buttons

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
        }
    }

    // Use OnTriggerExit2D to detect when the player exits the trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MoveOut();
            Debug.Log("Out");
        }
    }

    void MoveOut()
    {
        // Move Object off-screen using relative offset
        LeanTween.moveY(Object, targetTransform.position.y, duration).setEase(easeType); 
    }

    void MoveIn()
    {
        // Move Object back to its original position
        LeanTween.moveY(Object, targetOriginalTransform.position.y, duration).setEase(easeType);
    }
}
