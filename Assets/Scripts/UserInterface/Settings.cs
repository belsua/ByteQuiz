using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public TMP_Text nameText;
    public Sprite[] lockSprites;
    public Image[] statBars, lockImages;
    public TMP_Text[] statTexts;

    void Start()
    {
        UpdatePlayerInterface();
    }

    public void UpdatePlayerInterface()
    {
        if (SaveManager.player.stats.isNumberSystemUnlocked) lockImages[0].sprite = lockSprites[0];
        else lockImages[0].sprite = lockSprites[1];

        if (SaveManager.player.stats.isIntroProgrammingUnlocked) lockImages[1].sprite = lockSprites[0];
        else lockImages[1].sprite = lockSprites[1];

        nameText.text = SaveManager.player.profile.name;

        float[] playerStats = {
            SaveManager.player.stats.computerHistory,
            SaveManager.player.stats.computerElements,
            SaveManager.player.stats.numberSystem,
            SaveManager.player.stats.introProgramming
        };

        for (int i = 0; i < playerStats.Length; i++)
        {
            statBars[i].fillAmount = playerStats[i];
            statTexts[i].text = ((int)(playerStats[i] * 10000)).ToString();
        }
    }

}
