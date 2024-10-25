using TMPro;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;

public class ClassroomManager : MonoBehaviour
{
    [Header("Game Object References")]
    public TMP_InputField classroomIDInput;
    public TMP_Text classroomIDText, teacherNameText, statusText;
    public Button joinClassroomButton, leaveClassroomButton;
    public MenuManager menuManager;
    public GameObject savePanel;

    private DatabaseReference database;

    private void Awake()
    {
        database = FirebaseDatabase.DefaultInstance.RootReference;
    }

    #region Classroom System

    public void JoinClassroom()
    {
        string classroomID = classroomIDInput.text;
        if (!string.IsNullOrEmpty(classroomID)) AddPlayerToClassroom(classroomID);
        else menuManager.ShowErrorPanel("Please enter a classroom ID.");
    }

    private async void AddPlayerToClassroom(string classroomID)
    {
        menuManager.ShowLoadingPanel();
        // TODO: Check if classroom exists from Firebase before adding player to it
        await database.Child("classrooms").Child(classroomID).GetValueAsync().ContinueWithOnMainThread(task => 
        {
            if (task.IsFaulted)
            {
                //Debug.LogError("Failed to add player to classroom: " + task.Exception);
                menuManager.ShowErrorPanel("Failed to add player to classroom: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    //Debug.LogError("Classroom does not exist: " + classroomID);
                    menuManager.ShowErrorPanel("Classroom does not exist: " + classroomID);
                }
                else
                {
                    //Debug.Log("Classroom found: " + classroomID);
                    savePanel.GetComponentInChildren<TMP_Text>().text = "Select a save";
                    savePanel.GetComponent<SizeAnimate>().Open();
                }
            }

            menuManager.HideLoadingPanel();
        });

        //PlayerPrefs.SetString("ClassroomID", classroomID);
        //string playerID = player.profile.playerId;
        //string json = JsonConvert.SerializeObject(player);

        //await database.Child("classrooms").Child(classroomID).Child("players").Child(playerID).SetRawJsonValueAsync(json).ContinueWith(task =>
        //{
        //    if (task.IsCompleted) Debug.Log("Player added to classroom successfully.");
        //    else Debug.LogError("Failed to add player to classroom: " + task.Exception);
        //});

        //await SaveManager.instance.SavePlayerToFirebase(player);
    }

    public void UpdateClassroomInterface()
    {
        // Check if the player is not connected to the internet e
        if (Application.internetReachability.Equals(NetworkReachability.NotReachable))
        {
            // TODO: Show error dialog with a message that the player is not connected to the internet
            menuManager.ShowErrorPanel("Not connected to the internet.");
            Debug.Log("Not connected to the internet.");
        }
        else
        {
            GetClassroomInfo();
            Debug.Log("Connected to the internet.");
        }
    }

    private void GetClassroomInfo()
    {
        // Get the classroom ID from PlayerPrefs if it exists
        if (PlayerPrefs.HasKey("ClassroomID"))
        {
            classroomIDInput.enabled = false;
            joinClassroomButton.interactable = false;
            leaveClassroomButton.interactable = true;

            // TODO: Get classroom info from Firebase
        }
        else
        {
            classroomIDInput.enabled = true;
            joinClassroomButton.interactable = true;
            leaveClassroomButton.interactable = false;
        }
    }

    #endregion

}
