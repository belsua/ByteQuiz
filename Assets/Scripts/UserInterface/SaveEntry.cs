using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveEntry : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    //public TextMeshProUGUI coinsText;
    public Player player;
    FadeManager fadeManager;

    private void Awake()
    {
        fadeManager = GetComponent<FadeManager>();
    }

    public virtual void SetCharacterData(Player player)
    {
        this.player = player;
        nameText.text = player.name;
        //coinsText.text = player.coins.ToString();
    }

    public virtual void OnButtonClick()
    {
        StartCoroutine(TriggerButtonClick());
    }

    private IEnumerator TriggerButtonClick()
    {
        SaveManager.instance.player = player;
        yield return new WaitForSeconds(1);
        if (SaveManager.instance.multiplayer) SceneManager.LoadScene(2);
        else fadeManager.FadeToScene("Crib");
    }
}
