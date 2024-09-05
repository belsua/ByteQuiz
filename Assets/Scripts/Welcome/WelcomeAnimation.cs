using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomeAnimation : MonoBehaviour
{
    public GameObject Welcome;
    [SerializeField] RectTransform targetTransform;
    [SerializeField] float duration;
    [SerializeField] LeanTweenType easeType;
    
    //ADDED
    void Update()
    {
        if (Welcome.activeSelf) 
        {
            MoveY();
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
