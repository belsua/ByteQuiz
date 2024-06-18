using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject quizUI;

    public PlayManager playManager;

    public void SelectTopic(int topicIndex)
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
}
