using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Game Object References")]
    public GameObject contentParent;
    public GameObject itemPrefab;
    public GameObject loadingObject;
    public TextMeshProUGUI noItemsText;

    [Header("Topic Buttons")]
    public Button btnComputerElements;
    public Button btnComputerHistory;
    public Button btnIntroProgramming;
    public Button btnNumberSystem;

    [Header("Sprites")]
    public Sprite[] avatarSprites;
    public Sprite inactiveSprite;
    public Sprite activeSprite;

    private Button activeButton;

    void Start()
    {
        // Add listeners for the button clicks
        btnComputerElements.onClick.AddListener(() => OnButtonClicked(btnComputerElements));
        btnComputerHistory.onClick.AddListener(() => OnButtonClicked(btnComputerHistory));
        btnIntroProgramming.onClick.AddListener(() => OnButtonClicked(btnIntroProgramming));
        btnNumberSystem.onClick.AddListener(() => OnButtonClicked(btnNumberSystem));

        // Initialize the buttons to the inactive sprite
        ResetButtonSprites();
    }

    // Method to handle button clicks
    public void OnButtonClicked(Button clickedButton)
    {
        // If the clicked button is already active, do nothing
        if (activeButton == clickedButton)
            return;

        // Set all buttons to inactive sprite first
        ResetButtonSprites();

        // Set the clicked button to the active sprite
        clickedButton.image.sprite = activeSprite;

        // Store the clicked button as the current active button
        activeButton = clickedButton;

        // Here you would also trigger your specific data fetch (e.g., FetchData("computerElements"))
        // based on the button clicked. You can switch based on the clicked button:
        if (clickedButton == btnComputerElements)
        {
            FetchData("computerElements");
        }
        else if (clickedButton == btnComputerHistory)
        {
            FetchData("computerHistory");
        }
        else if (clickedButton == btnIntroProgramming)
        {
            FetchData("introProgramming");
        }
        else if (clickedButton == btnNumberSystem)
        {
            FetchData("numberSystem");
        }
    }

    // Method to reset all buttons to inactive sprite
    void ResetButtonSprites()
    {
        // Set all buttons to the inactive sprite
        btnComputerElements.image.sprite = inactiveSprite;
        btnComputerHistory.image.sprite = inactiveSprite;
        btnIntroProgramming.image.sprite = inactiveSprite;
        btnNumberSystem.image.sprite = inactiveSprite;
    }

    void FetchData(string key)
    {
        loadingObject.SetActive(true); // Show loading indicator
        ClearContent();

        SaveManager.instance.database.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            try
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Failed to fetch data: " + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    List<UserData> userDataList = new List<UserData>();

                    foreach (DataSnapshot userSnapshot in snapshot.Children)
                    {
                        string playerName = userSnapshot.Child("profile").Child("name").Value.ToString();
                        float value = float.Parse(userSnapshot.Child("stats").Child(key).Value.ToString());

                        // Set default avatarIndex to handle missing or null values
                        int avatarIndex = 0;  // Default avatar index if parsing fails or is null

                        DataSnapshot avatarSnapshot = userSnapshot.Child("profile").Child("avatar");
                        if (avatarSnapshot.Exists && avatarSnapshot.Value != null)
                        {
                            if (!int.TryParse(avatarSnapshot.Value.ToString(), out avatarIndex))
                            {
                                Debug.LogWarning($"Failed to parse avatar index for user {playerName}, using default avatar.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Avatar field is missing or null for user {playerName}, using default avatar.");
                        }

                        // Only add players with value > 0
                        if (value > 0)
                        {
                            userDataList.Add(new UserData(playerName, value, avatarIndex));
                        }
                    }

                    // Sort the list by value (descending order)
                    userDataList.Sort((x, y) => y.value.CompareTo(x.value));

                    // If there are no items to display, show the 'No Items' message
                    if (userDataList.Count == 0)
                    {
                        noItemsText.gameObject.SetActive(true);  // Show 'No items' message
                    }
                    else
                    {
                        noItemsText.gameObject.SetActive(false);  // Hide the message

                        // Instantiate prefabs using sorted data
                        foreach (UserData data in userDataList)
                        {
                            GameObject newItem = Instantiate(itemPrefab, contentParent.transform);

                            // Check if "Avatar" object and Image component exists
                            Image avatarImage = newItem.transform.Find("AvatarBackground/Mask/Avatar")?.GetComponent<Image>();
                            if (avatarImage == null)
                            {
                                Debug.LogError("Avatar object or Image component not found in prefab.");
                            }
                            else
                            {
                                // Set avatar sprite: use default avatar if avatarIndex is invalid
                                if (avatarSprites != null && data.avatarIndex >= 0 && data.avatarIndex < avatarSprites.Length)
                                {
                                    avatarImage.sprite = avatarSprites[data.avatarIndex];
                                }
                                else
                                {
                                    Debug.LogError("Avatar sprite array is null or index out of range.");
                                    avatarImage.sprite = avatarSprites[0];  // Make sure defaultAvatarSprite is also assigned
                                }
                            }

                            // Check if TextMeshProUGUI component exists
                            var textComponent = newItem.GetComponentInChildren<TextMeshProUGUI>();
                            if (textComponent == null)
                            {
                                Debug.LogError("TextMeshProUGUI component not found in prefab.");
                            }
                            else
                            {
                                textComponent.text = $"{data.playerName} - {data.value:F2}";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception occurred: " + ex.Message);
            }
            finally
            {
                loadingObject.SetActive(false); // Turn off loading indicator
            }
        });
    }

    // Clears the current scroll view content
    void ClearContent()
    {
        foreach (Transform child in contentParent.transform) Destroy(child.gameObject);
    }

    private class UserData
    {
        public string playerName;
        public float value;
        public int avatarIndex;

        public UserData(string playerName, float value, int avatarIndex)
        {
            this.playerName = playerName;
            this.value = value;
            this.avatarIndex = avatarIndex;
        }
    }
}


