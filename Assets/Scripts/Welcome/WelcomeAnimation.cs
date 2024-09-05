using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomeAnimation : MonoBehaviour
{
    public GameObject Welcome;
    [SerializeField] RectTransform targetTransform;
    [SerializeField] float duration;
    [SerializeField] LeanTweenType easeType;

    // New fields to control the behavior
    public bool checkActiveState; // true to check for active, false to check for inactive
    public bool moveAlongY; // true for MoveY, false for MoveX

    void Update()
    {
        // Check the selected active state
        if (checkActiveState ? Welcome.activeSelf : !Welcome.activeSelf)
        {
            // Move either along Y or X based on the user selection
            if (moveAlongY)
            {
                MoveY();
            }
            else
            {
                MoveX();
            }
        }
    }

    void MoveY()
    {
        LeanTween.moveY(gameObject, targetTransform.position.y, duration).setEase(easeType);
    }

    void MoveX()
    {
        LeanTween.moveX(gameObject, targetTransform.position.x, duration).setEase(easeType);
    }
}
