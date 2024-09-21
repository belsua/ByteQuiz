using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public GameObject canvas, saveEntryPrefab, scrollContent, exitPanel;

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
        if (Input.GetKeyUp(KeyCode.Escape)) exitPanel.SetActive(true);
    }

    void ResetObjectPositions()
    {
        foreach (Transform child in canvas.transform)
        {
            if (child.gameObject.name != "MenuPanel" && child.gameObject.name != "DeletePanel")
            {
                child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                child.localScale = Vector3.one;
                child.gameObject.SetActive(false);
            }
        }
    }

    [ContextMenu("Populate Save List")]
    public void PopulateSaveList()
    {
        foreach (Transform child in scrollContent.transform) { Destroy(child.gameObject); }
        List<Player> players = SaveManager.LoadPlayers();

        foreach (Player player in players)
        {
            GameObject entry = Instantiate(saveEntryPrefab, scrollContent.transform);
            SaveEntry saveEntry = entry.GetComponent<SaveEntry>();
            saveEntry.SetCharacterData(player);

            Button cloudButton = entry.transform.Find("Right/CloudButton").GetComponent<Button>();

            if (!PlayerPrefs.HasKey("CloudPlayerId"))
            {
                cloudButton.interactable = true;
            }
            else
            {
                if (player.profile.playerId == PlayerPrefs.GetString("CloudPlayerId")) // adds checkmark
                {
                    SpriteState spriteState = new()
                    {
                        highlightedSprite = null,
                        pressedSprite = saveEntry.checkSprites[3],
                        selectedSprite = null,
                        disabledSprite = saveEntry.checkSprites[4]
                    };

                    cloudButton.GetComponent<Image>().sprite = saveEntry.checkSprites[2];
                    cloudButton.spriteState = spriteState;
                    cloudButton.interactable = true;
                }
                else 
                {
                    cloudButton.interactable = false;
                }
            }
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
