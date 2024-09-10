using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizChecker : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject quizUI;
    public PlayManager playManager;

    public Button button;
    private bool collided = false;
    private bool pressed = false;

    public int topicIndex = 0;

    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        UpdateButtonInteractable();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collided = true;
            UpdateButtonInteractable();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            button.interactable = false;
            collided = false;
            CheckActivation();
        }
    }

    private void OnButtonClick()
    {
        if (button.interactable)
        {
            pressed = !pressed;
            CheckActivation();
        }
    }

    private void CheckActivation()
    {
        if (pressed && collided)
        {
            OpenQuizUI(topicIndex);
        }
    }

    private void OpenQuizUI(int topicIndex)
    {
        mainMenu.SetActive(false);
        quizUI.SetActive(true);
        playManager.SelectTopic(topicIndex);
    }

    public void BackToMenu()
    {
        quizUI.SetActive(false);
        mainMenu.SetActive(true);
    }

    private void UpdateButtonInteractable()
    {
        switch (topicIndex)
        {
            case 2: // Number System
                button.interactable = SaveManager.player.isNumberSystemUnlocked;
                break;
            case 3: // Intro to Programming
                button.interactable = SaveManager.player.isIntroProgrammingUnlocked;
                break;
            default:
                button.interactable = true;
                break;
        }
    }

    public void SetTopicIndex(int index)
    {
        topicIndex = index;
        UpdateButtonInteractable();
    }
}
