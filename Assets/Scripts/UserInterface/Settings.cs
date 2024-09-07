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
        if (SaveManager.player.isNumberSystemUnlocked) statusTexts[0].transform.parent.gameObject.SetActive(false);
        else statusTexts[0].text = "Locked";

        if (SaveManager.player.isIntroProgrammingUnlocked) statusTexts[1].transform.parent.gameObject.SetActive(false);
        else statusTexts[1].text = "Locked";

        nameText.text = SaveManager.player.name;

        float[] playerStats = {
            SaveManager.player.computerHistory,
            SaveManager.player.computerElements,
            SaveManager.player.numberSystem,
            SaveManager.player.introProgramming
        };

        for (int i = 0; i < playerStats.Length; i++)
        {
            statBars[i].fillAmount = playerStats[i];
            statTexts[i].text = ((int)(playerStats[i] * 10000)).ToString();
        }
    }

}
