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
    public Sprite[] lockSprites;
    public Image[] statBars, lockImages;
    public TMP_Text[] statTexts;
    [Space]
    public GameObject messagePanel;
    public AudioClip messageSound;
    public Button backButton;
    [Space]
    public Button[] numberSystemButtons;
    public Button[] introProgrammingButtons;
    public GameObject[] NSBookshelves, ITPBookshelves;
    public GameObject NSComputer, ITPComputer;
    public GameObject NSBookhelvesExclamation, ITPBookshelvesExclamation;
    public GameObject NSComputerExclamation, ITPComputerExclamation;
    
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
        if (SaveManager.player == null) saveManager.GetComponent<SaveManager>().CreatePlayer(3, "Debug guy", "Debug", 18, "Male");

        #endif

        GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>($"Controllers/{avatarAnimatorNames[SaveManager.player.profile.avatar]}");
        messageText = messagePanel.GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        #if DEBUG
        if (PlayerPrefs.GetInt("DebugMode", 0) == 1) debugButton.gameObject.SetActive(true);
        #endif

        SaveManager.player.OnStatUnlocked += ShowMessage;
        messagePanel.SetActive(false);

        UpdateUnlockedObjects();
        backButton.onClick.AddListener(() => StopAllCoroutines());
    }

    public void UpdateUnlockedObjects()
    {
        if (SaveManager.player.stats.isNumberSystemUnlocked) 
        { 
            foreach (Button button in numberSystemButtons) button.interactable = true;
            foreach (GameObject bookshelf in NSBookshelves) bookshelf.GetComponent<SpriteRenderer>().color = Color.white;
            NSComputer.GetComponent<SpriteRenderer>().color = Color.white;
            NSComputerExclamation.SetActive(true);
            NSBookhelvesExclamation.SetActive(true);
        }
        else 
        { 
            foreach (Button button in numberSystemButtons) button.interactable = false; 
            foreach (GameObject bookshelf in NSBookshelves) bookshelf.GetComponent<SpriteRenderer>().color = Color.gray;
            NSComputer.GetComponent<SpriteRenderer>().color = Color.gray;
            NSComputerExclamation.SetActive(false);
            NSBookhelvesExclamation.SetActive(false);
        }

        if (SaveManager.player.stats.isIntroProgrammingUnlocked) 
        { 
            foreach (Button button in introProgrammingButtons) button.interactable = true;
            foreach (GameObject bookshelf in ITPBookshelves) bookshelf.GetComponent<SpriteRenderer>().color = Color.white;
            ITPComputer.GetComponent<SpriteRenderer>().color = Color.white;
            ITPComputerExclamation.SetActive(true);
            ITPBookshelvesExclamation.SetActive(true);
        }
        else  
        { 
            foreach (Button button in introProgrammingButtons) button.interactable = false; 
            foreach (GameObject bookshelf in ITPBookshelves) bookshelf.GetComponent<SpriteRenderer>().color = Color.gray;
            ITPComputer.GetComponent<SpriteRenderer>().color = Color.gray;
            ITPComputerExclamation.SetActive(false);
            ITPBookshelvesExclamation.SetActive(false);
        }
    }

    public void ShowMessage(string message)
    {
        StopCoroutine(QuickShowMessageCoroutine("", Color.black));
        messageQueue.Enqueue(message);
        if (!isCoroutineRunning)  StartCoroutine(ShowMessageCoroutine());
    }

    IEnumerator ShowMessageCoroutine()
    {
        while (messageQueue.Count > 0)
        {
            isCoroutineRunning = true;
            string message = messageQueue.Dequeue();
            messageText.color = Color.black;
            messageText.text = message;
            AudioManager.PlaySound(messageSound);
            messagePanel.SetActive(true);
            yield return new WaitForSeconds(messageDelay);
            messagePanel.SetActive(false);
            isCoroutineRunning = false;
        }
    }

    // For single player quiz
    public void QuickShowMessage(string message, Color color)
    {
        StopAllCoroutines();
        StartCoroutine(QuickShowMessageCoroutine(message, color));
    }

    IEnumerator QuickShowMessageCoroutine(string message, Color color)
    {
        messageText.color = color;
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
