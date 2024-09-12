using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using Firebase.Database;
using System.Collections;

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

    DatabaseReference database;

    [Header("UI")]
    public TMP_InputField inputField;
    public GameObject errorPanel, creationPanel;
    public FadeManager fadeManager;

    internal bool multiplayer = false;

    #region Unity Methods

    private void Awake()
    {
        if (instance == null) {
            instance = this;
            deletePanel = GameObject.Find("DeletePanel");
        }
        else Destroy(gameObject);

        database = FirebaseDatabase.DefaultInstance.RootReference;
        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");

        transform.SetParent(null, false);
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Save System

    public void CreatePlayer(string name)
    {
        player = new Player(name);
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

    public static void SavePlayer(string playerId)
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
    }

    public void DeleteSave()
    {
        filePath = Path.Combine(saveFolder, $"{player.profile.playerId}.json");
        File.Delete(filePath);
        Debug.Log($"Player {player.profile.name} with id {player.profile.playerId} deleted");
        Destroy(selectedEntry);
    }

    // Firebase

    [ContextMenu("Save Player Data")]
    public void SavePlayerToFirebase()
    {
        string json = JsonConvert.SerializeObject(player);
        database.Child("users").Child(player.profile.playerId).SetRawJsonValueAsync(json);
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

    #endregion

    #region User Interface

    public void SetMode(bool state)
    {
        multiplayer = state;
    }

    public void ValidateAndCreate()
    {
        if (inputField.text.Length < 3)
        {
            errorPanel.SetActive(true);
        }
        else
        {
            CreatePlayer(inputField.text);
            fadeManager.FadeToScene("Crib");
        }
    }

    public static string[] GetSaveFiles()
    {
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
        return Directory.GetFiles(saveFolder, "*.json");
    }

    #endregion
}