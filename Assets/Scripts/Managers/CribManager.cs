using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CribManager : MonoBehaviour
{
    Player player;
    public TextMeshProUGUI nameText;
    //public TextMeshProUGUI coinsText;
    public Image computerHistory;
    public Image computerElements;
    public Image numberSystem;
    public Image introProgramming;

    private void Start()
    {
        player = SaveManager.player;
        LoadCharacterDetails();
    }

    void LoadCharacterDetails()
    {
        nameText.text = player.name;
        //coinsText.text = $"{player.coins} {(player.coins > 1 ? "coins" : "coin")}";
        computerHistory.fillAmount = player.computerHistory;
        computerElements.fillAmount = player.computerElements;
        numberSystem.fillAmount = player.numberSystem;
        introProgramming.fillAmount = player.introProgramming;
    }
}
