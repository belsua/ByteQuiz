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
    [Range(1, 5)]
    public int messageDelay = 3;

    private Queue<string> messageQueue = new();
    private bool isCoroutineRunning = false;
    readonly string[] avatarAnimatorNames = { "Adam", "Alex", "Bob", "Amelia" };

    private void Awake()
    {
        #if UNITY_EDITOR
        GameObject saveManager = new("NewObject");
        saveManager.AddComponent<SaveManager>();
        if (SaveManager.player == null) saveManager.GetComponent<SaveManager>().CreatePlayer(3, "Debug guy", "Debug", 18, "Male", "Debug Section");
        #endif
         
        GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>($"Controllers/{avatarAnimatorNames[SaveManager.player.profile.avatar]}");
    }

    private void Start()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
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
        if (SaveManager.player.stats.isNumberSystemUnlocked)
        {
            foreach (Button button in numberSystemButtons) button.interactable = true;
            statusTexts[0].transform.parent.gameObject.SetActive(false);
        }
        else
        {
            foreach (Button button in numberSystemButtons) button.interactable = false;
            statusTexts[0].text = "Locked";
        }
        
        if (SaveManager.player.stats.isIntroProgrammingUnlocked)
        {
            foreach (Button button in introProgrammingButtons) button.interactable = true;
            statusTexts[1].transform.parent.gameObject.SetActive(false);
        }
        else
        {
            foreach (Button button in introProgrammingButtons) button.interactable = false;
            statusTexts[1].text = "Locked";
        }
        
        nameText.text = SaveManager.player.profile.name;

        float[] playerStats = {
            SaveManager.player.stats.computerHistory,
            SaveManager.player.stats.computerElements,
            SaveManager.player.stats.numberSystem,
            SaveManager.player.stats.introProgramming
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
