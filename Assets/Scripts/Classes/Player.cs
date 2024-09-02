using UnityEngine;

public class Player
{
    public int slot;
    public string name;
    public float computerHistory;
    public float computerElements;
    public float numberSystem;
    public float introProgramming;

    public Player(string name, int slot)
    {
        this.name = name;
        this.slot = slot;

        computerHistory = 0.00f;
        computerElements = 0.00f;
        numberSystem = 0.00f;
        introProgramming = 0.00f;
    }

    public void IncreaseStat(string statName, float amount)
    {
        Debug.Log($"IncreaseStat called with {statName} and {amount}");

        switch (statName)
        {
            case "computerHistory":
            case "HOC":
                computerHistory += amount;
                break;
            case "computerElements":
            case "EOCS":
                computerElements += amount;
                break;
            case "numberSystem":
            case "NS":
                numberSystem += amount;
                break;
            case "introProgramming":
            case "ITP":
                introProgramming += amount;
                break;
            default:
                Debug.LogWarning($"Stat name {statName} not recognized.");
                break;
        }
    }
}
