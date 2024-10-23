using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SaveEntry : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Player player;
    public Sprite[] checkSprites;
    public Button cloudButton;
    GameObject deletePanel, loadingPanel, errorPanel;
    CanvasGroup loadingCanvasGroup;
    SpriteRenderer loadingSpriteRenderer;
    MenuManager menuManager;
    FadeManager fadeManager;


    private void Awake()
    {
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        deletePanel = GameObject.Find("DeletePanel");
        fadeManager = GetComponent<FadeManager>();
        loadingPanel = menuManager.loadingPanel;
        errorPanel = menuManager.errorPanel;
        loadingCanvasGroup = menuManager.loadingCanvasGroup;
        loadingSpriteRenderer = menuManager.loadingSpriteRenderer;
    }

    public virtual void SetCharacterData(Player player)
    {
        this.player = player;
        nameText.text = player.profile.name;
    }

    public virtual void OnButtonClick()
    {
        SaveManager.player = player;
        StartCoroutine(TriggerButtonClick());
    }

    private IEnumerator TriggerButtonClick()
    {
        yield return new WaitForSeconds(1);
        if (SaveManager.multiplayer) SceneManager.LoadScene(2);
        else fadeManager.FadeToScene("Crib");
    }

    public void OnDeleteButtonClick() 
    {
        Debug.Log($"Delete Button Clicked with {player.profile.playerId}");
        SaveManager.selectedEntry = gameObject;
        SaveManager.player = player;
        deletePanel.transform.position = new Vector3 (0, 0, 0);
        deletePanel.GetComponent<Canvas>().sortingOrder = 6;
        deletePanel.GetComponentInChildren<TextMeshProUGUI>().text = $"Delete {player.profile.name}?";
    }

    public async void OnCloudButtonClick()
    {
        if (!IsConnectedToInternet())
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<TMP_Text>().text = "Please check your internet connection and try again.";
            return;
        }

        try
        {
            ShowLoadingPanel();
            bool success = await CloudSave();

            if (!success)
            {
                errorPanel.SetActive(true);
                errorPanel.GetComponentInChildren<TMP_Text>().text = "Cloud save failed. Please try again.";
                return;
            }
            else 
            {
                await Task.Delay(1000);
            }
        }
        catch
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<TMP_Text>().text = $"An error occurred. Please try again.";
        }
        finally
        {
            HideLoadingPanel();
        }

    }

    public bool IsConnectedToInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public async Task<bool> CloudSave()
    {
        Button[] allButtons = FindObjectsOfType<Button>();

        try
        {
            if (cloudButton.spriteState.pressedSprite == checkSprites[1]) // unchecked
            {
                PlayerPrefs.SetString("CloudPlayerId", player.profile.playerId);
                await SaveManager.instance.SavePlayerToFirebase(player);

                SpriteState spriteState = new()
                {
                    highlightedSprite = null,
                    pressedSprite = checkSprites[3],
                    selectedSprite = null,
                    disabledSprite = checkSprites[4]
                };

                cloudButton.GetComponent<Image>().sprite = checkSprites[2];
                cloudButton.spriteState = spriteState;

                foreach (Button btn in allButtons)
                    if (btn.gameObject.name == "CloudButton" && btn.gameObject != cloudButton.gameObject) btn.interactable = false;
            }
            else // checked
            {
                await SaveManager.instance.DeletePlayerFromFirebase(player);
                PlayerPrefs.DeleteKey("CloudPlayerId");

                SpriteState spriteState = new()
                {
                    highlightedSprite = null,
                    pressedSprite = checkSprites[1],
                    selectedSprite = null,
                    disabledSprite = checkSprites[4]
                };

                cloudButton.GetComponent<Image>().sprite = checkSprites[0];
                cloudButton.spriteState = spriteState;

                foreach (Button btn in allButtons) btn.interactable = true;
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    private void ShowLoadingPanel()
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

}
