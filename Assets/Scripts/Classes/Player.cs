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

public class Player 
{ 
    public int slot;
    public string name;
    public bool needWelcome = true;

    public float computerHistory = 0;
    public float computerElements = 0;
    public float numberSystem = 0;
    public float introProgramming = 0;

    public const float toUnlockNumberSystem = 0.20f;
    public const float toUnlockIntroProgramming = 0.30f;
    public bool isNumberSystemUnlocked = false;
    public bool isIntroProgrammingUnlocked = false;

    public event System.Action<string> OnStatUnlocked;

    public Player(string name, int slot)
    {
        this.name = name;
        this.slot = slot;
    }

    public void IncreaseStat(string statName, float amount)
    {
        Debug.Log($"IncreaseStat called with {statName} and {amount}");

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

        CheckAndUnlockStats();
        SaveManager.SavePlayer(SaveManager.player.slot);
    }

    public void CheckAndUnlockStats()
    {
        if (computerHistory >= toUnlockNumberSystem)
        {
            if (!isNumberSystemUnlocked)
            {
                isNumberSystemUnlocked = true;
                OnStatUnlocked?.Invoke("Number System stat is now unlocked.");
                Debug.Log("Number System stat is now unlocked.");
            }
        }

        if (computerElements >= toUnlockIntroProgramming)
        {
            if (!isIntroProgrammingUnlocked)
            {
                isIntroProgrammingUnlocked = true;
                OnStatUnlocked?.Invoke("Intro to Programming stat is now unlocked.");
                Debug.Log("Intro to Programming stat is now unlocked.");
            }
        }
    }
}
