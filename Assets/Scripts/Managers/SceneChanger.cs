using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviourPunCallbacks
{
    readonly float timer = 1f;

    #region Photon Callbacks

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(false);
    }

    public override void OnLeftRoom()
    {
        StopAllCoroutines();
        PhotonNetwork.RemoveCallbackTarget(this);
        StartCoroutine(DelayLoadLevelCoroutine(2));
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon. Cause: " + cause.ToString());
    }

    IEnumerator DelayLoadLevelCoroutine(int i)
    {
        yield return new WaitForSeconds(timer);
        PhotonNetwork.LoadLevel(i);
    }

    #endregion

    public void ChangeScene(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void DelaySceneChange(int i)
    {
        StartCoroutine(DelaySceneChangeCoroutine(i));
    }

    public void DisconnectServer(int i)
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        StartCoroutine(DelaySceneChangeCoroutine(i));
    }

    IEnumerator DelaySceneChangeCoroutine(int i)
    {
        yield return new WaitForSeconds(timer);
        SceneManager.LoadScene(i);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
