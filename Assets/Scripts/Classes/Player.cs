using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// To increase player stats use IncreaseStat(string statName, float amount)
/// Here are the following stats: 
/// - History of Computer (HOC)
/// - Elements of Computer System (EOCS)
/// - Number System (NS)
/// - Intro to Programming (ITP)
/// At player creation the following stats is locked:
/// - Number System (Need to reach 20% of computer history)
/// - Intro to Programming (Need to reach 30% of computer history)
/// </summary>

[Serializable]
public class Player 
{
    public Profile profile;
    public Stats stats;
    public Dictionary<string, Dictionary<string, object>> activities = new();

    public event Action<string> OnStatUnlocked;

    public Player(int avatar, string name, string username, int age, string gender, string section)
    {
        profile = new(avatar, name, username, age, gender, section);
        stats = new Stats();
        activities = new();
    }

    public void SaveActivity(bool isMultiplayer, string topic, string score, string minigame = null, string[] players = null)
    {
        string formattedTopic = topic switch
        {
            "HOC" => "History of Computer",
            "EOCS" => "Elements of Computer System",
            "NS" => "Number System",
            "ITP" => "Intro to Programming",
            _ => topic,
        };

        Dictionary<string, object> activity = new()
        {
            { "date-time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") },
            { "mode", isMultiplayer ? "Multiplayer" : "Singleplayer" },
            { "topic", formattedTopic },
            { "score", score }
        };

        if (isMultiplayer)
        {
            activity["players"] = players;
            activity["minigame"] = minigame;
        }

        activities.Add(Guid.NewGuid().ToString(), activity);
    }

    public void IncreaseStat(string statName, float amount)
    {
        Debug.Log($"IncreaseStat called with {statName} and {amount}");

        if (statName == "NS" && stats.isNumberSystemUnlocked == false)
        {
            Debug.LogWarning("Number System stat is not unlocked.");
            return;
        }

        if (statName == "ITP" && stats.isIntroProgrammingUnlocked == false)
        {
            Debug.LogWarning("Intro to Programming stat is not unlocked.");
            return;
        }

        switch (statName)
        {
            case "HOC":
                stats.computerHistory += amount;
                break;
            case "EOCS":
                stats.computerElements += amount;
                break;
            case "NS":
                stats.numberSystem += amount;
                break;
            case "ITP":
                stats.introProgramming += amount;
                break;
            default:
                Debug.LogWarning($"Stat name {statName} not recognized.");
                break;
        }

        CheckAndUnlockStats();
        SaveManager.instance.SavePlayer(SaveManager.player.profile.playerId);
    }

    public void CheckAndUnlockStats()
    {
        if (stats.computerHistory >= 0.2f)
        {
            if (!stats.isNumberSystemUnlocked)
            {
                stats.isNumberSystemUnlocked = true;
                OnStatUnlocked?.Invoke("Number System stat is now unlocked.");
                Debug.Log("Number System stat is now unlocked.");
            }
        }

        if (stats.computerElements >= 0.3f)
        {
            if (!stats.isIntroProgrammingUnlocked)
            {
                stats.isIntroProgrammingUnlocked = true;
                OnStatUnlocked?.Invoke("Intro to Programming stat is now unlocked.");
                Debug.Log("Intro to Programming stat is now unlocked.");
            }
        }
    }


}

[Serializable]
public class Profile
{
    public string playerId;
    public int avatar;
    public string name;
    public string username;
    public int age;
    public string gender;
    public string section;

    public Profile(int avatar, string name, string username, int age, string gender, string section)
    {
        playerId = Guid.NewGuid().ToString();
        this.avatar = avatar;
        this.name = name;
        this.username = username;
        this.age = age;
        this.gender = gender;
        this.section = section;
    }
}

[Serializable]
public class Stats
{
    public bool needWelcome = true;
    public bool isNumberSystemUnlocked = false;
    public bool isIntroProgrammingUnlocked = false;
    public float computerHistory;
    public float computerElements;
    public float numberSystem;
    public float introProgramming;
}