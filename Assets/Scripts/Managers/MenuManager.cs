using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public GameObject canvas, saveEntryPrefab, scrollContent, exitPanel, loadingPanel, errorPanel;
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
            ResetObjectPositions();
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

        loadingCanvasGroup.alpha = 0;
        loadingSpriteRenderer.color = new Color(255, 255, 255, 0);
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

    void ResetObjectPositions()
    {
        foreach (Transform child in canvas.transform)
            if (child.gameObject.name != "MenuPanel" && child.gameObject.name != "DeletePanel")
                child.gameObject.SetActive(false);
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
}
