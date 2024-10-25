using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public GameObject canvas, saveEntryPrefab, scrollContent, exitPanel, deletePanel, loadingPanel, errorPanel;
    public CanvasGroup loadingCanvasGroup;
    public SpriteRenderer loadingSpriteRenderer;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public Sprite mutedSprite;
    public Sprite unmutedSprite;
    public Image buttonImage;
    
    private const string MuteKey = "AudioMuted";

    [Space]
    [Header("Debug")]
    public Button debugButton;
    public Sprite[] debugSprites;
    public bool isDebugEnabled;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PopulateSaveList();

            #if DEBUG
            // debug ui
            debugButton.gameObject.SetActive(true);
            isDebugEnabled = PlayerPrefs.GetInt("DebugMode", 0) == 1;
            UpdateDebugButton();
            debugButton.onClick.AddListener(ToggleDebug);
            #endif

            // audio
            bool isMuted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            SetAudioState(isMuted);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) exitPanel.SetActive(true);
    }

    #region Audio Settings

    public void ToggleAudio()
    {
        bool isMuted = audioSource.mute;
        SetAudioState(!isMuted);
    }

    private void SetAudioState(bool mute)
    {
        audioSource.mute = mute;

        PlayerPrefs.SetInt(MuteKey, mute ? 1 : 0);
        PlayerPrefs.Save();

        buttonImage.sprite = mute ? mutedSprite : unmutedSprite;
    }

    #endregion

    #region Debug Settings

    void ToggleDebug()
    {
        isDebugEnabled = !isDebugEnabled;
        PlayerPrefs.SetInt("DebugMode", isDebugEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateDebugButton();
    }

    void UpdateDebugButton()
    {
        debugButton.image.sprite = debugSprites[isDebugEnabled ? 1 : 0];
    }

    #endregion

    #region Photon Callbacks

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

    #region Context Menu Functions

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

    #endregion

    #region Loading Panel Functions

    public void ShowLoadingPanel()
    {
        loadingPanel.SetActive(true);
        LeanTween.alphaCanvas(loadingCanvasGroup, 1, 0).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.color(loadingSpriteRenderer.gameObject, Color.white, 0).setEase(LeanTweenType.easeInOutQuad);
    }

    public void HideLoadingPanel()
    {
        LeanTween.alphaCanvas(loadingCanvasGroup, 0, 1).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => loadingPanel.SetActive(false));
        LeanTween.color(loadingSpriteRenderer.gameObject, new Color(1, 1, 1, 0), 1).setEase(LeanTweenType.easeInOutQuad);
    }

    #endregion

    public void ShowErrorPanel(string message)
    {
        errorPanel.SetActive(true);
        errorPanel.GetComponentInChildren<TMP_Text>().text = message;
    }
}
