using TMPro;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ClassroomManager : MonoBehaviour
{
    [Header("Game Object References")]
    public TMP_InputField classroomIDInput;
    public TMP_Text classroomIDText, classroomNameText;
    public Button joinClassroomButton, leaveClassroomButton;
    public MenuManager menuManager;
    public Button backButton;
    public Color inactiveColor, activeColor;
    public Button detailTabButton, classroomTabButton;
    public GameObject detailUIContent, classmateUIContent;
    public GameObject playerItem;
    public GameObject infoPanel;
    private int selectedTab = 0;

    [Header("Save System")]
    public GameObject savePanel;
    public SaveEntry saveEntry;

    private DatabaseReference database;

    private void Awake()
    {
        database = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void Start()
    {
        LoadDetailsUI();
    }

    private void OnEnable()
    {
        detailTabButton.onClick.AddListener(OnDetailTabButtonClick);
        classroomTabButton.onClick.AddListener(OnClassroomTabButtonClick);
        SaveManager.instance.onClassroomChanged.AddListener(HandleClassroomChanged);
    }

    private void OnDisable()
    {
        detailTabButton.onClick.RemoveListener(OnDetailTabButtonClick);
        classroomTabButton.onClick.RemoveListener(OnClassroomTabButtonClick);
        SaveManager.instance.onClassroomChanged.RemoveListener(HandleClassroomChanged);
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
        infoPanel.SetActive(true);
    }

    private async void AddPlayerToClassroom(string classroomID)
    {
        // Clear the classroom ID input field before adding the player
        classroomIDInput.text = string.Empty;
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

                menuManager.HideLoadingPanel();
                foreach (Transform child in classmateUIContent.transform) Destroy(child.gameObject);
                detailUIContent.SetActive(false);

            }
        });
    }

    public async void UpdateClassroomInterface()
    {
        // Check if the player is not connected to the internet
        if (Application.internetReachability.Equals(NetworkReachability.NotReachable))
        {
            classroomIDInput.interactable = false;
            joinClassroomButton.interactable = false;
            leaveClassroomButton.interactable = false;
            infoPanel.SetActive(true);
            infoPanel.GetComponentInChildren<TMP_Text>().text = "You are not currently connected to the internet\nPlease try again later";
        }
        else
        {
            await GetClassroomInfo();
        }
    }

    [ContextMenu("Get Classroom Info")]
    private async Task GetClassroomInfo()
    {
        string classroomID = PlayerPrefs.GetString("ClassroomID", null);
        Dictionary<string, string> players = new();


        if (string.IsNullOrEmpty(classroomID))
        {
            Debug.Log("No classroom ID found.");
            EnableUIForJoining();
        }
        else
        {
            DisableUIForJoining();

            DataSnapshot snapshot = await database.Child("classrooms").Child(classroomID).GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.Log($"Classroom does not exist: {classroomID}, returning.");
                menuManager.ShowErrorPanel($"Hey! Something happen to your classroom. Ask your teacher for help.");

                // Reset classroom ID in PlayerPrefs and enable UI for joining a classroom
                // And update classroom details
                PlayerPrefs.DeleteKey("ClassroomID");
                EnableUIForJoining();
                UpdateClassroomDetails();

                return;
            }
            else if (snapshot.HasChild("name") && snapshot.HasChild("teacherID"))
            {
                string nameValue = snapshot.Child("name").GetValue(true).ToString();

                // Handle players text
                if (snapshot.HasChild("players"))
                {
                    foreach (DataSnapshot playerSnapshot in snapshot.Child("players").Children)
                    {
                        if (playerSnapshot.HasChild("profile") && playerSnapshot.Child("profile").HasChild("name"))
                        {
                            players.Add(playerSnapshot.Key, playerSnapshot.Child("profile").Child("name").GetValue(true).ToString());
                        }
                    }
                }
                else
                {
                    Debug.Log("Players: None");
                }

                UpdateUI(classroomID, nameValue, players);
            }
        }    
    }

    // Helper method to update UI text fields
    private void UpdateUI(string classroomID, string classroomName, Dictionary<string, string> players)
    {
        classroomIDText.text = classroomID;
        classroomNameText.text = classroomName;

        foreach (Transform child in classmateUIContent.transform) Destroy(child.gameObject);
        foreach (KeyValuePair<string, string> player in players)
        {
            GameObject playerItemPrefab = Instantiate(playerItem, classmateUIContent.transform);
            playerItemPrefab.transform.Find("LabelText (TMP)").GetComponent<TMP_Text>().text = "Classmate Name";
            playerItemPrefab.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = player.Value;
        }
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

    #region Tab Functions

    public void OnDetailTabButtonClick()
    {
        selectedTab = 0;
        UpdateTabColor();
        LoadDetailsUI();
    }

    public void OnClassroomTabButtonClick()
    {
        selectedTab = 1;
        UpdateTabColor();
        LoadClassroomUI();
    }

    private void UpdateTabColor()
    {
        if (selectedTab == 0)
        {
            detailTabButton.GetComponent<Image>().color = activeColor;
            classroomTabButton.GetComponent<Image>().color = inactiveColor;
        }
        else
        {
            classroomTabButton.GetComponent<Image>().color = activeColor;
            detailTabButton.GetComponent<Image>().color = inactiveColor;
        }
    }

    private void LoadDetailsUI()
    {
        Debug.Log("Loading details UI");

        classmateUIContent.SetActive(false);
        detailUIContent.SetActive(true);
        UpdateClassroomDetails();
    }

    private void UpdateClassroomDetails()
    {
        string classroomID = PlayerPrefs.GetString("ClassroomID", null);
        if (string.IsNullOrEmpty(classroomID))
        {
            foreach (Transform child in detailUIContent.transform) child.gameObject.SetActive(false);
            foreach (Transform child in classmateUIContent.transform) child.gameObject.SetActive(false);
            if (infoPanel != null) infoPanel.SetActive(true);
        }
        else
        {
            foreach (Transform child in detailUIContent.transform) child.gameObject.SetActive(true);
            if (infoPanel != null) infoPanel.SetActive(false);
        }
    }

    private void LoadClassroomUI()
    {
        Debug.Log("Loading classroom UI");

        detailUIContent.SetActive(false);
        classmateUIContent.SetActive(true);
    }

    private void HandleClassroomChanged()
    {
        Debug.Log("Classroom changed");

        if (infoPanel != null) infoPanel.SetActive(false);
        UpdateClassroomDetails();
        savePanel.GetComponent<SizeAnimate>().Close();
    }
    #endregion
}
