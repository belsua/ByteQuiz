using UnityEngine;
using UnityEngine.UI;

public class DeleteButton : MonoBehaviour
{
    Button deleteButton;

    private void Awake()
    {
        deleteButton = GetComponent<Button>();
    }

    private void Start()
    {
        deleteButton.onClick.AddListener(SaveManager.instance.OnDeleteButtonPressed);
    }
}
