using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text[] statTexts, statusTexts;
    public Image[] statBars;

    void Start()
    {
        UpdatePlayerInterface();
    }

    public void UpdatePlayerInterface()
    {
        if (SaveManager.player.stats.isNumberSystemUnlocked) statusTexts[0].transform.parent.gameObject.SetActive(false);
        else statusTexts[0].text = "Locked";

        if (SaveManager.player.stats.isIntroProgrammingUnlocked) statusTexts[1].transform.parent.gameObject.SetActive(false);
        else statusTexts[1].text = "Locked";

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
