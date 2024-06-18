using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    [Serializable]
    public class Achievement
    {
        public string name;
        public string description;
        public bool unlocked;
    }

    public List<Achievement> achievements;

    private void Start()
    {
        LoadAchievements();
    }

    public void UnlockAchievement(string name)
    {
        Achievement achievement = achievements.Find(x => x.name == name);
        if (achievement != null && !achievement.unlocked) 
        { 
            achievement.unlocked = true;
            Debug.Log($"Achievement unlocked: {achievement.name}");
            
            // UI stuff here

        }
    }

    #region Achievement Operations

    public void LoadAchievements()
    {
        string path = $"{Application.persistentDataPath}/achievements.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, this);
        }
        else
        {
            // Create achievement file if doesn't exists
            string json = JsonUtility.ToJson(this, true);
            File.WriteAllText(path, json);
        }
    }

    public void SaveAchievements()
    {
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText($"{Application.persistentDataPath}/achievements.json", json);
    }

    #endregion
}
