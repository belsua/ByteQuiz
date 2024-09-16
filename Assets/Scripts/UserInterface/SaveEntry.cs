using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveEntry : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Player player;
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
        deletePanel.GetComponentInChildren<TextMeshProUGUI>().text = $"Are you sure you want to delete {player.profile.name}?";
    }
}
