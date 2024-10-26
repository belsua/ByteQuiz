using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DeleteButton : MonoBehaviour
{
    public MenuManager menuManager;
    public ClassroomManager classroomManager;
    Button deleteButton;

    private void Awake()
    {
        deleteButton = GetComponent<Button>();
    }

    private void Start()
    {
        deleteButton.onClick.AddListener(OnDeleteButtonClick);
    }

    private async void OnDeleteButtonClick()
    {
        try
        {
            menuManager.ShowLoadingPanel();

            bool success = await SaveManager.instance.DeleteSave();

            if (!success)
            {
                menuManager.ShowErrorPanel("Failed to save. Please try again.");
                return;
            }
            else
            {
                await Task.Delay(1000);
            }
        }
        catch
        {
            menuManager.ShowErrorPanel("An error occurred. Please try again.");
        }
        finally
        {
            classroomManager.UpdateClassroomInterface();
            menuManager.PopulateSaveList();
            menuManager.HideLoadingPanel();
        }
    }
}
