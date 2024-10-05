using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Firebase.Database;
using System.Collections;
using System.Threading.Tasks;

/// <summary>
/// Stores the player data at `public static Player player`
/// To access it anywhere, use `SaveManager.player`
/// To write the save file, use `SaveManager.SavePlayer()`
/// To read the save file, use `SaveManager.LoadPlayer()`
/// To delete the save file, use `SaveManager.DeleteSave()`
/// To access the save files, use `SaveManager.GetSaveFiles()`
/// To check if a save file exists, use `SaveManager.SaveExists()`
/// To access the save folder, use `SaveManager.saveFolder`
/// </summary>

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public static GameObject deletePanel, selectedEntry;
    public static Player player;
    public static string filePath;
    public static string saveFolder;
    public DatabaseReference database;

    public static bool multiplayer = false;

    #region Unity Methods

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        database = FirebaseDatabase.DefaultInstance.RootReference;
        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");

        transform.SetParent(null, false);
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Save System

    public void CreatePlayer(int avatar, string name, string username, int age, string gender)
    {
        player = new Player(avatar, name, username, age, gender);
        SavePlayer(player.profile.playerId);
        Debug.Log("Player created: " + player.profile.playerId);
    }

    public static Player LoadPlayer(int slot) 
    { 
        string path = GetSaveFiles()[slot];
        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<Player>(json);
    }

    public static List<Player> LoadPlayers()
    {
        List<Player> players = new();
        string[] files = GetSaveFiles();

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            Player player = JsonConvert.DeserializeObject<Player>(json);
            players.Add(player);
        }

        return players;
    }

    public async void SavePlayer(string playerId)
    {
        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

        filePath = Path.Combine(saveFolder, $"{playerId}.json");

        var settings = new JsonSerializerSettings
        {
            FloatFormatHandling = FloatFormatHandling.DefaultValue,
            NullValueHandling = NullValueHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(player, settings);
        File.WriteAllText(filePath, json);
        Debug.Log($"Player data saved to {filePath}");
        if (PlayerPrefs.GetString("CloudPlayerId") == playerId) await SavePlayerToFirebase(player);
        else Debug.Log("Player is not selected to save to cloud.");
    }

    public async void OnDeleteButtonPressed()
    {
        await DeleteSave();
    }

    public async Task DeleteSave()
    {
        if (PlayerPrefs.GetString("CloudPlayerId") == player.profile.playerId) DeleteAllPrefs();

        string filePath = Path.Combine(saveFolder, $"{player.profile.playerId}.json");

        // Attempt to delete the local file with proper error handling
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Player {player.profile.name} with id {player.profile.playerId} deleted from local storage.");
            }
            else
            {
                Debug.LogWarning($"Player file at {filePath} not found, nothing to delete locally.");
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error deleting local player file: {ex.Message}");
        }

        // Destroy the UI element or game object associated with the entry
        if (selectedEntry != null)
        {
            Destroy(selectedEntry);
            Debug.Log("Selected entry destroyed.");
        }
        else
        {
            Debug.LogWarning("No selected entry found to destroy.");
        }

        // Delete the player from Firebase
        try
        {
            await DeletePlayerFromFirebase(player);  // Await to ensure it completes
            Debug.Log("Player successfully deleted from Firebase.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to delete player from Firebase: {ex.Message}");
        }
    }

    // Firebase

    [ContextMenu("Save Player Data")]
    public async Task SavePlayerToFirebase(Player player)
    {
        try
        {
            // Serialize player data to JSON
            string json = JsonConvert.SerializeObject(player);

            // Save to Firebase
            await database.Child("users").Child(player.profile.playerId).SetRawJsonValueAsync(json);

            Debug.Log("Player data saved successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save player data: {ex.Message}");
        }
    }

    [ContextMenu("Load Player Data")]
    public void LoadPlayerFromFirebase()
    {
        StartCoroutine(LoadPlayerFromFirebaseCoroutine());
    }

    IEnumerator LoadPlayerFromFirebaseCoroutine()
    {

        var data = database.Child("users").Child(player.profile.playerId).GetValueAsync();
        yield return new WaitUntil(predicate: () => data.IsCompleted);
        Debug.Log(data.Result.GetRawJsonValue());

        DataSnapshot snapshot = data.Result;
        string json = snapshot.GetRawJsonValue();

        if (json != null) player = JsonConvert.DeserializeObject<Player>(json);
        else Debug.Log("No data available");
    }

    public async Task DeletePlayerFromFirebase(Player player)
    {
        // Ensure player and playerId are not null
        if (player == null || string.IsNullOrEmpty(player.profile.playerId))
        {
            Debug.LogError("Player or playerId is null, cannot delete from Firebase");
            return;
        }

        try
        {
            // Try to delete the player data
            await database.Child("users").Child(player.profile.playerId).RemoveValueAsync();

            // Log success
            Debug.Log($"Player {player.profile.name} with id {player.profile.playerId} deleted from Firebase successfully.");
        }
        catch (System.Exception ex)
        {
            // Handle errors
            Debug.LogError($"Failed to delete player {player.profile.name} with id {player.profile.playerId} from Firebase. Error: {ex.Message}");
        }
    }

    [ContextMenu("Delete All Player Prefs")]
    public void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [ContextMenu("Reset Current Player")]
    public void ResetPlayer()
    {
        player = new Player(player.profile.avatar, player.profile.name, player.profile.username, player.profile.age, player.profile.gender, player.profile.playerId);
        SavePlayer(player.profile.playerId);
    }

    #endregion

    #region User Interface

    public static void SetMode(bool state)
    {
        multiplayer = state;
    }

    public static string[] GetSaveFiles()
    {
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
        return Directory.GetFiles(saveFolder, "*.json");
    }

    #endregion
}