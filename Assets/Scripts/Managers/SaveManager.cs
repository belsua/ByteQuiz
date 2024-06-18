using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public TMP_InputField inputField;
    public GameObject errorPanel;
    public GameObject creationPanel;
    public GameObject scrollContent;
    public GameObject saveEntryPrefab;
    public Player player;
    public FadeManager fadeManager;

    public bool multiplayer = false;

    private string filePath;
    private string saveFolder;

    #region Unity Methods

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
    }

    private void Start()
    {
        PopulateSaveList(saveEntryPrefab, scrollContent);
    }

    #endregion

    #region Save System

    void CreatePlayer(string name, int slot)
    {
        player = new Player(name);
        SavePlayer(slot);
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

    void SavePlayer(int slot)
    {
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

        filePath = Path.Combine(saveFolder, $"save-{slot}.json");
        string json = JsonUtility.ToJson(player, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"Player data saved to {filePath}");
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

    string[] GetSaveFiles()
    {
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
        return Directory.GetFiles(saveFolder, "*.json");
    }

    #endregion
}