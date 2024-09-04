using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CribManager : MonoBehaviour
{
    [Header("User Interface")]
    public TextMeshProUGUI nameText;
    public Image computerHistory;
    public Image computerElements;
    public Image numberSystem;
    public Image introProgramming;
    [Space]
    public Button[] numberSystemButtons;
    public Button[] introProgrammingButtons;
    [Header("Debug")]
    public Button debugButton;
    public GameObject debugPanel;
    public bool showWelcome = false;

    private void Awake()
    {
        #if UNITY_EDITOR
        SaveManager.player ??= new Player("Debug guy", 0);
        SaveManager.player.needWelcome = showWelcome;
        #endif
    }

    private void Start()
    {
        #if UNITY_EDITOR
        debugButton.gameObject.SetActive(true);
        debugPanel.transform.position = debugPanel.transform.parent.position;
        debugPanel.SetActive(false);
        #endif

        UpdatePlayerInterface();
    }

    public void UpdatePlayerInterface()
    {
        if (SaveManager.player.isNumberSystemUnlocked) foreach (Button button in numberSystemButtons) button.interactable = true;
        else foreach (Button button in numberSystemButtons) button.interactable = false;
        
        if (SaveManager.player.isIntroProgrammingUnlocked) foreach (Button button in introProgrammingButtons) button.interactable = true;

        else foreach (Button button in introProgrammingButtons) button.interactable = false;
        nameText.text = SaveManager.player.name;
        computerHistory.fillAmount = SaveManager.player.computerHistory;
        computerElements.fillAmount = SaveManager.player.computerElements;
        numberSystem.fillAmount = SaveManager.player.numberSystem;
        introProgramming.fillAmount = SaveManager.player.introProgramming;
    }
}
