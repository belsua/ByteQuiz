using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Events;
using System.Collections.Generic;

public class SaveEntry : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Player player;
    public Sprite[] checkSprites;
    public Button cloudButton;
    GameObject deletePanel, errorPanel, savePanel;
    MenuManager menuManager;
    FadeManager fadeManager;
    ClassroomManager classroomManager;

    private void Awake()
    {
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        deletePanel = menuManager.deletePanel;
        fadeManager = GetComponent<FadeManager>();
        errorPanel = menuManager.errorPanel;
        savePanel = GameObject.Find("SavePanel");
        classroomManager = GameObject.Find("ClassroomManager").GetComponent<ClassroomManager>();
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
        if (SaveManager.multiplayer) savePanel.GetComponent<SizeAnimate>().Close(() => SceneManager.LoadScene(2));
        else savePanel.GetComponent<SizeAnimate>().Close(() => fadeManager.FadeToScene("Crib"));
    }

    public void OnDeleteButtonClick() 
    {
        Debug.Log($"Delete Button Clicked with {player.profile.playerId}");
        SaveManager.selectedEntry = gameObject;
        SaveManager.player = player;

        deletePanel.SetActive(true);
        deletePanel.transform.position = new Vector3(0, 0, 0);
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
            menuManager.ShowLoadingPanel();
            bool success = await CloudSave();

            if (!success)
            {
                menuManager.ShowErrorPanel("Failed to save. Please try again.");
                return;
            }
            else 
            {
                await Task.Delay(1000);
            }
        }
        catch
        {
            menuManager.ShowErrorPanel("An error occurred. Please try again.");
        }
        finally
        {
            classroomManager.UpdateClassroomInterface();
            menuManager.PopulateSaveList();
            menuManager.HideLoadingPanel();
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
                if (PlayerPrefs.HasKey("ClassroomID")) await SaveManager.instance.SavePlayerToClassroomFirebase(PlayerPrefs.GetString("ClassroomID"), player);
                else await SaveManager.instance.SavePlayerToFirebase(player);

                PlayerPrefs.SetString("CloudPlayerId", player.profile.playerId);

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
                if (PlayerPrefs.HasKey("ClassroomID")) await SaveManager.instance.DeletePlayerFromClassroomFirebase(PlayerPrefs.GetString("ClassroomID"));
                else await SaveManager.instance.DeletePlayerFromFirebase(player);

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
}
