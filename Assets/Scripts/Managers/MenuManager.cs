using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class MenuManager : MonoBehaviourPunCallbacks
{
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) Quit();
    }

    public void ChangeScene(int i) 
    {
        StartCoroutine(DelaySceneChange(i));
    }

    IEnumerator DelaySceneChange(int i)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(i);
    }

    public void DisconnectServer(int i)
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        StartCoroutine(DelaySceneChange(i));
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(false);
    }

    public override void OnLeftRoom() {
        StopAllCoroutines();
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.LoadLevel(2);
    }

    public void Quit()
    {
        StartCoroutine(DelayQuit());
    }

    IEnumerator DelayQuit()
    {
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon. Cause: " + cause.ToString());
    }
}
