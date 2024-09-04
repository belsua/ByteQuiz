using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public CribManager cribManager;

    public void DebugIncrease(string topic)
    {
        SaveManager.player.IncreaseStat(topic, 0.10f);
        cribManager.UpdatePlayerInterface();
    }

    public void DebugUnlock(string topic)
    {
        Debug.Log($"Unlocked {topic}");

        switch (topic) {
            case "NS":
                SaveManager.player.isNumberSystemUnlocked = true;
                break;
            case "ITP":
                SaveManager.player.isIntroProgrammingUnlocked = true;
                break;
            default:
                Debug.LogWarning($"Unknown topic: {topic}");
                break;
        }

        SaveManager.SavePlayer(SaveManager.player.slot);
        cribManager.UpdatePlayerInterface();
    }
}
    
