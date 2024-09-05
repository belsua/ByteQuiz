using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public CribManager cribManager;

    public void DebugIncrease(string topic)
    {
        SaveManager.player.IncreaseStat(topic, 0.10f);
        cribManager.ShowMessage($"Your {topic} stat increased!");
        cribManager.UpdatePlayerInterface();
    }
}
    
