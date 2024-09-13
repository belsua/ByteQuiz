using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviourPunCallbacks
{
    GameObject canvas, saveEntryPrefab, scrollContent;
    public static GameObject deletePanel;

    public void Awake()
    {

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            canvas = GameObject.Find("Canvas");
            saveEntryPrefab = Resources.Load<GameObject>("UI/SaveEntry");
            scrollContent = GameObject.Find("Content");
            deletePanel = GameObject.Find("DeletePanel");
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            ResetObjectPositions();
            PopulateSaveList();
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) Quit();
    }

    void ResetObjectPositions()
    {
        foreach (Transform child in canvas.transform)
        {
            if (child.gameObject.name != "MenuPanel")
            {
                child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                child.localScale = Vector3.one;
                child.gameObject.SetActive(false);
            }
        }
    }

    public void PopulateSaveList()
    {
        List<Player> players = SaveManager.LoadPlayers();

        foreach (Player player in players)
        {
            GameObject entry = Instantiate(saveEntryPrefab, scrollContent.transform);
            SaveEntry saveEntry = entry.GetComponent<SaveEntry>();
            saveEntry.SetCharacterData(player);
        }
    }


    #region Scene Management

    public void ChangeScene(int i) 
    {
        StartCoroutine(DelaySceneChange(i));
    }

    IEnumerator DelaySceneChange(int i)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(i);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        StartCoroutine(DelayQuit());
        #endif
    }

    IEnumerator DelayQuit()
    {
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    #endregion

    #region Photon Callbacks

    public void DisconnectServer(int i)
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        StartCoroutine(DelaySceneChange(i));
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(false);
    }

    public override void OnLeftRoom()
    {
        StopAllCoroutines();
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.LoadLevel(2);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon. Cause: " + cause.ToString());
    }

    #endregion
}
