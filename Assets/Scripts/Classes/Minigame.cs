using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

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

    protected AudioClip finishClip, correctClip, wrongClip;
    protected GameObject playerPrefab, messagePanel, quizPanel, buttons;
    protected TMP_Text messageText, questionText;
    protected AudioSource AudioSource;
    protected Image questionImage;

    protected int currentQuestionIndex;
    protected string topic;
    protected int score;
    //protected int seed;

    [Header("Values")]
    [TextArea] public string message;
    [SerializeField] Vector2 min, max;
    [SerializeField] float startTime = 10.0f;

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

        finishClip = Resources.Load<AudioClip>("Audio/finish-clip");
        correctClip = Resources.Load<AudioClip>("Audio/correct-clip");
        wrongClip = Resources.Load<AudioClip>("Audio/wrong-clip");

        //#if UNITY_EDITOR
        //Debug.Log(finishClip ? "finishClip found" : "finishClip not found");
        //Debug.Log(correctClip ? "correctClip found" : "correctClip not found");
        //Debug.Log(wrongClip ? "wrongClip found" : "wrongClip not found");
        //#endif
    }

    protected virtual void Start()
    {
        #if UNITY_EDITOR
        if (!PhotonNetwork.IsConnected)
        {
            SaveManager.selectedPlayer = new Player("Player", 0);
            PhotonNetwork.LocalPlayer.NickName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}";
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.JoinRoom("test");
        }
        #endif

        if (PhotonNetwork.IsMasterClient)
        {
            int temp = Random.Range(0, 10);
            photonView.RPC("SetSeedRPC", RpcTarget.All, temp);
        }

        quizPanel.SetActive(false);

        SpawnPlayers();
        FreezeAllPlayers();
        TriggerCountdown(message);
    }

    #region Question Functions

    protected QuestionDatabase LoadQuestionData(string topic, int seed, int limit = 10)
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

            // Parameter to limit the number of questions
            originalData.questions = originalData.questions.OrderBy(x => rng.Next()).Take(limit).ToArray();
        }

        return Instantiate(originalData);
    }

    int seed;
    protected void ReceiveSelectedTopic()
    {
        //#if UNITY_EDITOR
        topic = "EOCS";
        questionData = LoadQuestionData(topic, seed);
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    seed = Random.Range(0, 10);
        //    Debug.Log($"Seed from ReceiveSelectedTopic: {seed}");
        //    photonView.RPC("SetSeedRPC", RpcTarget.All, seed);
        //}
        //questionData = LoadQuestionData(topic, seed);
//#else
//        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("selectedTopic"))
//        {
//            topic = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedTopic"];
//            if (PhotonNetwork.IsMasterClient)
//            {
//                 int seed = Random.Range(0, LoadQuestionData(topic).questions.Length);
//                //questionData = LoadQuestionData(topic);
//                // Sync the number of questions to all clients
//                photonView.RPC("SetSeedRPC", RpcTarget.All, topic, seed);
//            }
//        }
//        else
//        {
//            Debug.LogError("Selected topic not found in CustomProperties.");
//        }
//#endif
    }

    [PunRPC]
    public void SetSeedRPC(int temp)
    {
        seed = temp;
        Debug.Log($"Name: {PhotonNetwork.LocalPlayer.NickName}, Seed from SetSeedRPC: {seed}");
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

    #region Abstract Functions

    public abstract void StartMinigame();
    public abstract void EndGame();
    public virtual void AnswerCorrect() { }
    public virtual void AnswerWrong() { }

    #endregion

    #region Spawn Functions

    public virtual void SpawnPlayers(int order = 0)
    {
        playerPrefab = Resources.Load<GameObject>("Player");
        playerPrefab.GetComponent<SpriteRenderer>().sortingOrder = order;
        Vector2 position = new(Random.Range(min.x, max.x), Random.Range(min.y, max.y));

        if (!PhotonNetwork.IsConnected) player = Instantiate(playerPrefab, position, Quaternion.identity);
        else player = PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);

        Debug.Log(PhotonNetwork.LocalPlayer.NickName);
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
