using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;

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

    [Header("UI")]
    public TMP_InputField inputField;
    public GameObject canvas, errorPanel, creationPanel, scrollContent, saveEntryPrefab;
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

        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
    }

    private void Start()
    {
        ResetObjectPositions();
        PopulateSaveList(saveEntryPrefab, scrollContent);
    }

    #endregion

    #region Save System

    public static void CreatePlayer(string name, int slot, bool needWelcome = true)
    {
        player = new Player(name, slot) { needWelcome = needWelcome };
        SavePlayer(slot);
    }

    public static Player LoadPlayer(int slot) 
    { 
        string path = GetSaveFiles()[slot];
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<Player>(json);
    }

    List<Player> LoadPlayers()
    {
        List<Player> players = new();
        string[] files = GetSaveFiles();

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            Player player = JsonUtility.FromJson<Player>(json);
            players.Add(player);
        }

        return players;
    }

    public static void SavePlayer(int slot)
    {
        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

        filePath = Path.Combine(saveFolder, $"save-{slot}.json");

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
        filePath = Path.Combine(saveFolder, $"save-{player.slot}.json");
        File.Delete(filePath);
        Debug.Log($"Player {player.name} with slot {player.slot} deleted");
        Destroy(selectedEntry);
    }

    int GetNextAvailableSlot()
    {
        int slot = 0;
        while (SaveExists(slot)) slot++;
        return slot;
    }

    bool SaveExists(int slot)
    {
        string filePath = Path.Combine(saveFolder, $"save-{slot}.json");
        return File.Exists(filePath);
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
            CreatePlayer(inputField.text, GetNextAvailableSlot());
            fadeManager.FadeToScene("Crib");
        }
    }

    public void PopulateSaveList(GameObject prefab, GameObject content)
    {
        List<Player> players = LoadPlayers();

        foreach (Player player in players)
        {
            GameObject entry = Instantiate(prefab, content.transform);
            SaveEntry saveEntry = entry.GetComponent<SaveEntry>();
            saveEntry.SetCharacterData(player);
        }
    }

    public static string[] GetSaveFiles()
    {
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
        return Directory.GetFiles(saveFolder, "*.json");
    }

    void ResetObjectPositions()
    {
        foreach (Transform child in canvas.transform)
        {
            if (child.gameObject.name != "MenuPanel")
            {
                child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                child.localScale = Vector3.one;
                child.gameObject.SetActive(false);
            }
        }
    }

    #endregion
}