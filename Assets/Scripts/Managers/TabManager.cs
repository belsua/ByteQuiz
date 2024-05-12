using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    public GameObject[] Tabs;
    public Image[] TabButtons;
    public Sprite ActiveTabSprite, InactiveTabSprite;
    public Color ActiveColor, InactiveColor;

    public void TabSwitch(int i)
    {
        foreach (GameObject tab in Tabs) 
            tab.SetActive(false);

        Tabs[i].SetActive(true);

        foreach (Image button in TabButtons)
        {
            button.sprite = InactiveTabSprite;
            button.transform.GetChild(0).GetComponent<Image>().color = InactiveColor;
        }

        TabButtons[i].sprite = ActiveTabSprite;
        TabButtons[i].transform.GetChild(0).GetComponent<Image>().color = ActiveColor;
    }
}
