using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveEntry : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Player player;
    FadeManager fadeManager;

    private void Awake()
    {
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
        if (SaveManager.instance.multiplayer) SceneManager.LoadScene(2);
        else fadeManager.FadeToScene("Crib");
    }

    public void OnDeleteButtonClick() 
    {
        SaveManager.selectedEntry = gameObject;
        SaveManager.player = player;
        MenuManager.deletePanel.transform.position = new Vector3 (0, 0, 0);
        MenuManager.deletePanel.SetActive(true);
        MenuManager.deletePanel.GetComponentInChildren<TextMeshProUGUI>().text = "Are you sure you want to delete " + player.profile.name + "?";
    }
}
