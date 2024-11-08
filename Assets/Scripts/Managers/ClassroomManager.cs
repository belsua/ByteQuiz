using TMPro;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Text;
using System.Threading.Tasks;

public class ClassroomManager : MonoBehaviour
{
    [Header("Game Object References")]
    public TMP_InputField classroomIDInput;
    public TMP_Text classroomIDText, teacherNameText, playersText, statusText;
    public Button joinClassroomButton, leaveClassroomButton;
    public MenuManager menuManager;
    public Button backButton;

    public GameObject savePanel;
    public SaveEntry saveEntry;

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

    public void LeaveClassroom()
    {
        string classroomID = PlayerPrefs.GetString("ClassroomID");
        if (!string.IsNullOrEmpty(classroomID)) RemovePlayerFromClassroom(classroomID);
        else menuManager.ShowErrorPanel("You are not in a classroom.");
        classroomIDInput.text = string.Empty;
    }

    private async void AddPlayerToClassroom(string classroomID)
    {
        PlayerPrefs.DeleteKey("CloudPlayerId");
        menuManager.PopulateSaveList();
        menuManager.ShowLoadingPanel();

        // Check if classroom exists from Firebase before adding player to it
        await database.Child("classrooms").Child(classroomID).GetValueAsync().ContinueWithOnMainThread(task => 
        {
            if (task.IsFaulted)
            {
                menuManager.ShowErrorPanel("Failed to add player to classroom: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    menuManager.ShowErrorPanel("Classroom does not exist: " + classroomID);
                }
                else
                {
                    PlayerPrefs.SetString("ClassroomID", classroomID);

                    savePanel.GetComponentInChildren<TMP_Text>().text = "Press cloud button";
                    savePanel.GetComponent<SizeAnimate>().Open();

                    Button[] buttons = savePanel.GetComponentsInChildren<Button>();

                    foreach (Button button in buttons)
                    {
                        if (button.name == "CloudButton") continue;

                        if (button.name == "DeleteButton" || button.name == "BackButton") button.interactable = true;
                        else if (button.name != "CloudButton") button.interactable = false;
                    }
                }
            }

            UpdateClassroomInterface();
            menuManager.HideLoadingPanel();
        });
    } 

    private async void RemovePlayerFromClassroom(string classroomID)
    {
        menuManager.ShowLoadingPanel(); 

        // Check if classroom exists from Firebase before removing player from it
        await database.Child("classrooms").Child(classroomID).GetValueAsync().ContinueWithOnMainThread(async task => 
        {
            if (task.IsFaulted)
            {
                menuManager.ShowErrorPanel("Failed to remove player from classroom: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    menuManager.ShowErrorPanel("Classroom does not exist: " + classroomID);
                }
                else
                {
                    await SaveManager.instance.DeletePlayerFromClassroomFirebase(classroomID);

                    PlayerPrefs.DeleteKey("ClassroomID");
                    PlayerPrefs.DeleteKey("CloudPlayerId");

                    await GetClassroomInfo();
                }

                //menuManager.PopulateSaveList();
                menuManager.HideLoadingPanel();
            }
        });
    }

    public async void UpdateClassroomInterface()
    {
        // Check if the player is not connected to the internet e
        if (Application.internetReachability.Equals(NetworkReachability.NotReachable)) menuManager.ShowErrorPanel("Player is not connected to the internet.");
        else await GetClassroomInfo();
    }

    [ContextMenu("Get Classroom Info")]
    private async Task GetClassroomInfo()
    {
        string classroomID = PlayerPrefs.GetString("ClassroomID", null);
        Debug.Log($"Classroom ID: {classroomID}");

        if (string.IsNullOrEmpty(classroomID))
        {
            UpdateUI("Classroom ID: None", "Teacher: None", "Players: None", "Status: Not joined");
            EnableUIForJoining();
            //Debug.Log("Classroom ID is null, returning.");
            //return;
        }
        else
        {
            DisableUIForJoining();

            DataSnapshot snapshot = await database.Child("classrooms").Child(classroomID).GetValueAsync();

            if (!snapshot.Exists)
            {
                menuManager.ShowErrorPanel($"Classroom does not exist: {classroomID}");
                Debug.Log($"Classroom does not exist: {classroomID}, returning.");
                return;
            }
            else if (snapshot.HasChild("name") && snapshot.HasChild("teacherID"))
            {
                string nameValue = snapshot.Child("name").GetValue(true).ToString();
                string teacherName = snapshot.Child("teacherID").GetValue(true).ToString();
                UpdateUI($"Classroom ID: {classroomID}", $"Teacher: {teacherName}", "Players: None", "Status: Joined");

                // Handle players text
                if (snapshot.HasChild("players"))
                {
                    StringBuilder playersList = new StringBuilder("Players: ");
                    foreach (DataSnapshot playerSnapshot in snapshot.Child("players").Children)
                    {
                        if (playerSnapshot.HasChild("profile") && playerSnapshot.Child("profile").HasChild("name"))
                        {
                            string playerName = playerSnapshot.Child("profile/name").GetValue(true).ToString();
                            playersList.Append(playerName + ", ");
                        }
                    }
                    if (playersList.Length > 10)
                    {
                        playersList.Length -= 2;
                    }
                    playersText.text = playersList.ToString();
                }
                else
                {
                    playersText.text = "Players: None";
                }
            }
        }    
    }

    //if (classroomID == null)
    //{
    //    UpdateUI("Classroom ID: None", "Teacher: None", "Players: None", "Status: Not joined");
    //    EnableUIForJoining();
    //    Debug.Log("Classroom ID is null, returning.");
    //    return;
    //}
    //else
    //{
    //    DisableUIForJoining();

    //    try
    //    {
    //        DataSnapshot snapshot = await database.Child("classrooms").Child(classroomID).GetValueAsync();

    //        if (!snapshot.Exists)
    //        {
    //            menuManager.ShowErrorPanel($"Classroom does not exist: {classroomID}");
    //            Debug.Log($"Classroom does not exist: {classroomID}, returning.");
    //            return;
    //        }
    //        else if (snapshot.HasChild("name") && snapshot.HasChild("teacherID"))
    //        {
    //            string nameValue = snapshot.Child("name").GetValue(true).ToString();
    //            string teacherName = snapshot.Child("teacherID").GetValue(true).ToString();
    //            UpdateUI($"Classroom ID: {classroomID}", $"Teacher: {teacherName}", "Players: None", "Status: Joined");

    //            // Handle players text
    //            if (snapshot.HasChild("players"))
    //            {
    //                StringBuilder playersList = new StringBuilder("Players: ");
    //                foreach (DataSnapshot playerSnapshot in snapshot.Child("players").Children)
    //                {
    //                    if (playerSnapshot.HasChild("profile") && playerSnapshot.Child("profile").HasChild("name"))
    //                    {
    //                        string playerName = playerSnapshot.Child("profile/name").GetValue(true).ToString();
    //                        playersList.Append(playerName + ", ");
    //                    }
    //                }
    //                if (playersList.Length > 10)
    //                {
    //                    playersList.Length -= 2;
    //                }
    //                playersText.text = playersList.ToString();
    //            }
    //            else
    //            {
    //                playersText.text = "Players: None";
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"Failed to get classroom data: {ex.Message}");
    //    }
    //}


    // Helper method to update UI text fields
    private void UpdateUI(string classroomIDText, string teacherNameText, string playersText, string statusText)
    {
        this.classroomIDText.text = classroomIDText;
        this.teacherNameText.text = teacherNameText;
        this.playersText.text = playersText;
        this.statusText.text = statusText;
    }

    private void EnableUIForJoining()
    {
        classroomIDInput.enabled = true;
        joinClassroomButton.interactable = true;
        classroomIDInput.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        leaveClassroomButton.interactable = false;
    }

    private void DisableUIForJoining()
    {
        classroomIDInput.enabled = false;
        joinClassroomButton.interactable = false;
        classroomIDInput.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
        leaveClassroomButton.interactable = true;
    }

    #endregion

}
