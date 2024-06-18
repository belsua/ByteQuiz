using UnityEngine;
using TMPro;
using Photon.Pun;

public class CountdownTimer : MonoBehaviourPunCallbacks
{
    //public GameObject countdownPanel, settingsPanel;
    //public float startTime = 10.0f;
    //public TMP_Text countdownText;

    //private float currentTime;
    //private bool timerStarted = false;

    //private void Update()
    //{
    //    if (timerStarted)
    //    {
    //        // Update the countdown timer
    //        if (currentTime > 0)
    //        {
    //            currentTime -= Time.deltaTime;
    //            countdownText.text = $"Game starts! Choosing a game in {Mathf.Ceil(currentTime)}";
    //        }
    //        else
    //        {
    //            // Countdown finished
    //            timerStarted = false;
    //            // Optionally, trigger an event or call a method when the countdown ends
    //            OnCountdownEnd();
    //        }
    //    }
    //}

    //private void OnCountdownEnd()
    //{
    //    RoomManager.Re
    //}

    //public void TriggerCountdown()
    //{
    //    if (PhotonNetwork.IsMasterClient)
    //        photonView.RPC("StartCountdown", RpcTarget.AllBuffered);
    //}

    //[PunRPC]
    //private void StartCountdown()
    //{
    //    currentTime = startTime;
    //    countdownPanel.SetActive(true);
    //    settingsPanel.SetActive(false);
    //    timerStarted = true; 
    //}
}
