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
    GameObject deletePanel;
    FadeManager fadeManager;

    private void Awake()
    {
        deletePanel = GameObject.Find("DeletePanel");
        fadeManager = GetComponent<FadeManager>();
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
        await CloudSave();
    }

    public async Task CloudSave()
    {
        Button[] allButtons = FindObjectsOfType<Button>();

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
    }
}
