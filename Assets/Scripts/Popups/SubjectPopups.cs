using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubjectPopups : MonoBehaviour
{
    public GameObject Object;
    [SerializeField] RectTransform targetTransform;
    [SerializeField] RectTransform targetOriginalTransform;
    [SerializeField] float duration;
    [SerializeField] LeanTweenType easeType;

    public PlayerController playerController; // Reference to the PlayerController script

    private float screenWidth; // Variable to store the screen width
    private float movementOffset; // The calculated offset for moving buttons

    // Enum for subject types
    public enum Subject { NumberSystem, IntroProgramming };
    public Subject subjectType;

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
            if (ShouldAnimate())
            {
                MoveIn();
            }
        }
    }

    // Use OnTriggerExit2D to detect when the player exits the trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ShouldAnimate())
            {
                MoveOut();
            }
        }
    }

    // Function to check if the subject is unlocked or locked
    private bool ShouldAnimate()
    {
        switch (subjectType)
        {
            case Subject.NumberSystem:
                return !SaveManager.player.stats.isNumberSystemUnlocked;
            case Subject.IntroProgramming:
                return !SaveManager.player.stats.isIntroProgrammingUnlocked;
            default:
                return true; // Default case: animate if there's no specific subject
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
