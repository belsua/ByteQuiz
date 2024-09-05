using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CribManager : MonoBehaviour
{
    [Header("User Interface")]
    public TextMeshProUGUI nameText;
    public Image[] statBars;
    public TMP_Text[] statTexts, statusTexts;
    [Space]
    public GameObject messagePanel;
    public AudioClip messageSound;
    [Space]
    public Button[] numberSystemButtons;
    public Button[] introProgrammingButtons;
    [Header("Debug")]
    public Button debugButton;
    public GameObject debugPanel;
    [Tooltip("Show welcome panel on start? Only works when opening Crib scene directly not coming from main menu.")]
    public bool showWelcome = false;
    [Range(1, 5)]
    public int messageDelay = 3;

    private Queue<string> messageQueue = new();
    private bool isCoroutineRunning = false;

    private void Awake()
    {
        #if UNITY_EDITOR
        if (SaveManager.player == null) SaveManager.CreatePlayer("Debug guy", 999, showWelcome);
        #endif
    }

    private void Start()
    {
        #if UNITY_EDITOR
        debugButton.gameObject.SetActive(true);
        debugPanel.transform.position = debugPanel.transform.parent.position;
        debugPanel.SetActive(false);
        #endif

        SaveManager.player.OnStatUnlocked += ShowMessage;
        messagePanel.SetActive(false);

        UpdatePlayerInterface();
    }

    public void UpdatePlayerInterface()
    {
        if (SaveManager.player.isNumberSystemUnlocked)
        {
            foreach (Button button in numberSystemButtons) button.interactable = true;
            statusTexts[0].transform.parent.gameObject.SetActive(false);
        }
        else
        {
            foreach (Button button in numberSystemButtons) button.interactable = false;
            statusTexts[0].text = "Locked";
        }
        
        if (SaveManager.player.isIntroProgrammingUnlocked)
        {
            foreach (Button button in introProgrammingButtons) button.interactable = true;
            statusTexts[1].transform.parent.gameObject.SetActive(false);
        }
        else
        {
            foreach (Button button in introProgrammingButtons) button.interactable = false;
            statusTexts[1].text = "Locked";
        }
        
        nameText.text = SaveManager.player.name;

        float[] playerStats = {
            SaveManager.player.computerHistory,
            SaveManager.player.computerElements,
            SaveManager.player.numberSystem,
            SaveManager.player.introProgramming
        };

        for (int i = 0; i < playerStats.Length; i++)
        {
            statBars[i].fillAmount = playerStats[i];
            statTexts[i].text = ((int)(playerStats[i] * 10000)).ToString();
        }
    }

    public void ShowMessage(string message)
    {
        messageQueue.Enqueue(message);
        if (!isCoroutineRunning)  StartCoroutine(ShowMessageCoroutine());
    }

    IEnumerator ShowMessageCoroutine()
    {
        while (messageQueue.Count > 0)
        {
            isCoroutineRunning = true;
            string message = messageQueue.Dequeue();
            messagePanel.GetComponentInChildren<TMP_Text>().text = message;
            AudioManager.PlaySound(messageSound);
            messagePanel.SetActive(true);
            yield return new WaitForSeconds(messageDelay);
            messagePanel.SetActive(false);
            isCoroutineRunning = false;
        }
    }
}
