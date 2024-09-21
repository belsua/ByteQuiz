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
    public Button backButton;
    [Space]
    public Button[] numberSystemButtons;
    public Button[] introProgrammingButtons;
    [Header("Debug")]
    public Button debugButton;
    public GameObject debugPanel;
    [Range(1, 5)]
    public int messageDelay = 3;
    [Range(1, 10)]
    public int closeTimer = 5;

    private Queue<string> messageQueue = new();
    private bool isCoroutineRunning = false;
    readonly string[] avatarAnimatorNames = { "Adam", "Alex", "Bob", "Amelia" };
    TMP_Text messageText;

    private void Awake()
    {
        #if UNITY_EDITOR
        GameObject saveManager = new("NewObject");
        saveManager.AddComponent<SaveManager>();
        SaveManager.player ??= SaveManager.LoadPlayer(0);
        if (SaveManager.player == null) saveManager.GetComponent<SaveManager>().CreatePlayer(3, "Debug guy", "Debug", 18, "Male", "Debug Section");

#endif

        GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>($"Controllers/{avatarAnimatorNames[SaveManager.player.profile.avatar]}");
        messageText = messagePanel.GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        debugButton.gameObject.SetActive(true);
        debugPanel.transform.position = debugPanel.transform.parent.position;
        debugPanel.SetActive(false);
        #endif

        SaveManager.player.OnStatUnlocked += QuickShowMessage;
        messagePanel.SetActive(false);

        UpdatePlayerInterface();
        backButton.onClick.AddListener(() => StopAllCoroutines());
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
        StopCoroutine(QuickShowMessageCoroutine(""));
        messageQueue.Enqueue(message);
        if (!isCoroutineRunning)  StartCoroutine(ShowMessageCoroutine());
    }

    IEnumerator ShowMessageCoroutine()
    {
        while (messageQueue.Count > 0)
        {
            isCoroutineRunning = true;
            string message = messageQueue.Dequeue();
            messageText.text = message;
            AudioManager.PlaySound(messageSound);
            messagePanel.SetActive(true);
            yield return new WaitForSeconds(messageDelay);
            messagePanel.SetActive(false);
            isCoroutineRunning = false;
        }
    }

    public void QuickShowMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(QuickShowMessageCoroutine(message));
    }

    IEnumerator QuickShowMessageCoroutine(string message)
    {
        messageText.text = message;
        messagePanel.SetActive(true);
        yield return new WaitForSeconds(messageDelay);
        messagePanel.SetActive(false);
    }


    public void MessageCloseCountdown()
    {
        StartCoroutine(MessageCloseCountdownCoroutine());
    }

    IEnumerator MessageCloseCountdownCoroutine()
    {
        yield return new WaitForSeconds((messageDelay * 2) + 1); // For the message showing 2 times with 1 second delay
        messagePanel.SetActive(true);
        int time = closeTimer;
        while (time > 0)
        {
            messageText.text = $"Closing in {time}...";
            yield return new WaitForSeconds(1);
            time--;
        }

        messagePanel.SetActive(false);
        backButton.onClick.Invoke();
    }

}
