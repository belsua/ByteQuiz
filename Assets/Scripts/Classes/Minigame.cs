using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;

public abstract class Minigame : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab, messagePanel;
    public TMP_Text messageText;
    [SerializeField] float startTime = 10.0f;
    [SerializeField] Vector2 min, max;

    bool timerStarted = false;
    float currentTime;

    private void Start()
    {
        SpawnPlayers();
        FreezeAllPlayers();
        TriggerCountdown();
    }

    void Update()
    {
        TimerCheck();
    }

    #region Abstract Functions

    public abstract void StartGame();
    public abstract void EndGame();

    #endregion

    #region Spawn Functions

    void SpawnPlayers()
    {
        Vector2 position = new(Random.Range(min.x, max.x), Random.Range(min.y, max.y));

        if (!PhotonNetwork.IsConnected) Instantiate(playerPrefab, position, Quaternion.identity);
        else PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
    }

    #endregion

    #region Countdown Functions

    void TriggerCountdown()
    {
        photonView.RPC("StartCountdown", RpcTarget.AllBuffered);
    }


    [PunRPC] public void StartCountdown()
    {
        currentTime = startTime;
        timerStarted = true;
    }

    void TimerCheck()
    {
        if (timerStarted)
            if (currentTime > 0) messageText.text = $"Escape the maze by answering the questions! Starts in {Mathf.Ceil(currentTime -= Time.deltaTime)}....";
            else StartCoroutine(OnCountdownEnd());
    }

    IEnumerator OnCountdownEnd()
    {
        timerStarted = false;
        messageText.text = "GO!";
        UnfreezeAllPlayers();
        StartGame();
        yield return new WaitForSeconds(3.0f);
        messagePanel.SetActive(false);
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

}
