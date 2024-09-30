using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public CribManager cribManager;
    public Settings settings;

    public void DebugIncrease(string topic)
    {
        if (!SaveManager.player.stats.isIntroProgrammingUnlocked && topic == "ITP") cribManager.ShowMessage($"ITP is locked!");
        if (!SaveManager.player.stats.isNumberSystemUnlocked && topic == "NS") cribManager.ShowMessage($"NS is locked!");

        SaveManager.player.IncreaseStat(topic, 0.10f);
        cribManager.UpdateButtons();
        settings.UpdatePlayerInterface();
    }
}
    
