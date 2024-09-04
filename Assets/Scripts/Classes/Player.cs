using System.IO;
using UnityEngine;

public class Player 
{ 
    public int slot;
    public string name;
    public bool needWelcome = true;

    public float computerHistory = 0;
    public float computerElements = 0;
    public float numberSystem = 0;
    public float introProgramming = 0;

    public const float toUnlockNumberSystem = 20f;
    public const float toUnlockIntroProgramming = 30f;
    public bool isNumberSystemUnlocked = false;
    public bool isIntroProgrammingUnlocked = false;

    public Player(string name, int slot)
    {
        this.name = name;
        this.slot = slot;
    }

    public void IncreaseStat(string statName, float amount)
    {
        if (statName == "NS" && isNumberSystemUnlocked == false)
        {
            Debug.LogWarning("Number System stat is not unlocked.");
            return;
        }

        if (statName == "ITP" && isIntroProgrammingUnlocked == false)
        {
            Debug.LogWarning("Intro to Programming stat is not unlocked.");
            return;
        }

        CheckAndUnlockStats();

        switch (statName)
        {
            case "HOC":
                computerHistory += amount;
                break;
            case "EOCS":
                computerElements += amount;
                break;
            case "NS":
                numberSystem += amount;
                break;
            case "ITP":
                introProgramming += amount;
                break;
            default:
                Debug.LogWarning($"Stat name {statName} not recognized.");
                break;
        }

        Debug.Log($"IncreaseStat called with {statName} and {amount}");
        if (Directory.Exists(SaveManager.saveFolder)) SaveManager.SavePlayer(SaveManager.player.slot);
    }

    private void CheckAndUnlockStats()
    {
        if (computerHistory >= toUnlockNumberSystem)
        {
            if (!isNumberSystemUnlocked)
            {
                isNumberSystemUnlocked = true;
                Debug.Log("Number System stat is now unlocked.");
            }
        }

        if (computerElements >= toUnlockNumberSystem)
        {
            if (!isIntroProgrammingUnlocked)
            {
                isIntroProgrammingUnlocked = true;
                Debug.Log("Intro to Programming stat is now unlocked.");
            }
        }
    }
}
