using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using System.IO;
//using System.Runtime.CompilerServices;
//using System.IO;
//using Newtonsoft.Json;

public interface IMinigame
{
    void AnswerCorrect();
    void AnswerWrong();
}

[RequireComponent(typeof(PhotonView), typeof(AudioSource))]
public abstract class Minigame : MonoBehaviourPunCallbacks, IMinigame
{
    public QuestionDatabase questionData;
    public static GameObject player;

    protected AudioClip finishClip, correctClip, wrongClip, roundClip, upClip;
    protected GameObject playerPrefab, messagePanel, quizPanel, buttons;
    protected TMP_Text messageText, questionText;
    protected AudioSource AudioSource;
    protected Image questionImage;

    protected string topic;
    protected string playerName;
    protected int seed;
    protected int currentQuestionIndex;
    protected int score = 0;
    protected int correct = 0;

    [Header("Values")]
    [TextArea] public string message;
    [SerializeField] Vector2 min, max;
    [SerializeField] float startTime = 10.0f;
    [SerializeField] [Range(1, 10)] protected int returnTime = 5;
    [SerializeField] protected int total = 10;
    
    [Header("Objects")]
    [SerializeField] protected GameObject[] options;

    protected virtual void Awake()
    {
        quizPanel = GameObject.Find("Quiz");
        messagePanel = GameObject.Find("MessagePanel");
        messageText = messagePanel.GetComponentInChildren<TMP_Text>();
        questionText = GameObject.Find("QuestionText").GetComponent<TMP_Text>();
        questionImage = GameObject.Find("QuestionImage").GetComponent<Image>();
        buttons = GameObject.Find("Buttons");
        AudioSource = GetComponent<AudioSource>();

        finishClip = Resources.Load<AudioClip>("Audio/Sound/finish-clip");
        correctClip = Resources.Load<AudioClip>("Audio/Sound/correct-clip");
        wrongClip = Resources.Load<AudioClip>("Audio/Sound/wrong-clip");
        roundClip = Resources.Load<AudioClip>("Audio/Sound/round-clip");
        upClip = Resources.Load<AudioClip>("Audio/Sound/up-clip");
    }

    protected virtual void Start()
    {
        #if UNITY_EDITOR
        SaveManager.saveFolder ??= Path.Combine(Application.persistentDataPath, "Saves");
        SaveManager.player ??= SaveManager.LoadPlayer(0);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = $"Player {SaveManager.player.name}";
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.JoinOrCreateRoom("test", LobbyManager.roomOptions, TypedLobby.Default);
        }
        #endif

        //string playerFilePath = Path.Combine(SaveManager.saveFolder, $"save-{SaveManager.player.slot}.json");
        //string jsonString = File.ReadAllText(playerFilePath);
        //SaveManager.player = JsonConvert.DeserializeObject<Player>(jsonString);
        //SaveManager.player.IncreaseStat(topic, 100);
        //jsonString = JsonConvert.SerializeObject(SaveManager.player, Formatting.Indented);
        //File.WriteAllText(playerFilePath, jsonString);
        //Debug.Log("JSON data overwritten successfully.");

        playerName = PhotonNetwork.LocalPlayer.NickName;
        quizPanel.SetActive(false);
        SpawnPlayers();
        FreezeAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            int seed = Random.Range(0, 10);
            photonView.RPC("SetSeedRPC", RpcTarget.All, seed);
            TriggerCountdown(message);
        }

    }

    #region Virtual Functions

    public virtual void StartMinigame() { }
    public virtual void EndGame() { }
    public virtual void InitializePlayerData() { }
    public virtual void AnswerCorrect() { }
    public virtual void AnswerWrong() { }

    #endregion

    #region Question Functions

    protected QuestionDatabase LoadQuestionData(string topic, int seed, int limit)
    {
        QuestionDatabase originalData = Resources.Load<QuestionDatabase>($"Single Player Quiz/{topic}");

        if (originalData == null)
        {
            Debug.LogError($"Failed to load QuizData for topic: {topic}");
            return null;
        }
        else
        {
            System.Random rng = new(seed);
            originalData.questions = originalData.questions.OrderBy(x => rng.Next()).Take(limit).ToArray();
        }

        return Instantiate(originalData);
    }

    protected void ReceiveSelectedTopic()
    {
#if UNITY_EDITOR
        topic = "EOCS";
        questionData = LoadQuestionData(topic, seed, total);
#else
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("selectedTopic"))
        {
            topic = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedTopic"];
            questionData = LoadQuestionData(topic, seed, total);
        }
        else
        {
            Debug.LogError("Selected topic not found in CustomProperties.");
        }
#endif
    }

    [PunRPC]
    public void SetSeedRPC(int seed)
    {
        this.seed = seed;
    }

    protected void GenerateQuestions()
    {
        for (int i = 0; i < questionData.questions.Length; i++)
        {
            currentQuestionIndex = i;

            Sprite sprite = questionData.questions[currentQuestionIndex].questionImage;

            if (sprite != null)
            { 
                SetImageProperties(sprite, 1);
                questionText.text = string.Empty;
            } 
            else
            {
                SetImageProperties(null, 0);
                questionText.text = questionData.questions[currentQuestionIndex].questionText;
            }

            SetAnswers();
        }
    }

    private void SetImageProperties(Sprite sprite, float alpha)
    {
        questionImage.sprite = sprite;
        Color color = questionImage.color;
        color.a = alpha;
        questionImage.color = color;
    }

    protected virtual void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<Answers>().isCorrect = false;
            options[i].GetComponentInChildren<TMP_Text>().text = questionData.questions[currentQuestionIndex].answers[i];

            if (questionData.questions[currentQuestionIndex].correctAnswerIndex == i)
                options[i].GetComponent<Answers>().isCorrect = true;
        }
    }

    protected void RemoveQuestion(int index)
    {
        for (int i = index + 1; i < questionData.questions.Length; i++)
            questionData.questions[i - 1] = questionData.questions[i];

        System.Array.Resize(ref questionData.questions, questionData.questions.Length - 1);
    }

    #endregion

    #region Spawn Functions

    public virtual void SpawnPlayers(int order = 0)
    {
        playerPrefab = Resources.Load<GameObject>("Player");
        playerPrefab.GetComponent<SpriteRenderer>().sortingOrder = order;
        Vector2 position = new(Random.Range(min.x, max.x), Random.Range(min.y, max.y));

        if (!PhotonNetwork.IsConnected) player = Instantiate(playerPrefab, position, Quaternion.identity);
        else player = PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
    }

    #endregion

    #region Freeze Functions

    void FreezeAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerControllerMultiplayer movement = player.GetComponent<PlayerControllerMultiplayer>();
            if (movement != null) movement.isFrozen = true;
        }
    }

    void UnfreezeAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerControllerMultiplayer movement = player.GetComponent<PlayerControllerMultiplayer>();
            if (movement != null) movement.isFrozen = false;
        }
    }

    #endregion

    #region Countdown Functions

    public void TriggerCountdown(string message)
    {
        photonView.RPC("StartCountdownRPC", RpcTarget.All, message);
    }


    [PunRPC] public void StartCountdownRPC(string message)
    {
        #if UNITY_EDITOR
        startTime = 2.0f;
        #endif

        StartCoroutine(StartCountdown(message));
    }

    IEnumerator StartCountdown(string message)
    {
        int time = (int)startTime;
        while (time > 0)
        {
            messageText.text = $"{message} Starts in {time}....";
            yield return new WaitForSeconds(1.0f);
            time--;
        }

        messageText.text = "GO!";
        UnfreezeAllPlayers();
        StartMinigame();
        yield return new WaitForSeconds(3.0f);
        messagePanel.SetActive(false);
    }

    #endregion
}
