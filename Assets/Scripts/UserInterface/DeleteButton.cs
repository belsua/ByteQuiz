using UnityEngine;
using UnityEngine.UI;

public class DeleteButton : MonoBehaviour
{
    public MenuManager menuManager;
    Button deleteButton;

    private void Awake()
    {
        deleteButton = GetComponent<Button>();
    }

    private void Start()
    {
        deleteButton.onClick.AddListener(OnDeleteButtonClick);
    }

    private void OnDeleteButtonClick()
    {
        SaveManager.instance.OnDeleteButtonPressed();
        menuManager.PopulateSaveList();
    }
}
