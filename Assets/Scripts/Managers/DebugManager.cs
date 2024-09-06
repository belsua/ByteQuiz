using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public CribManager cribManager;

    public void DebugIncrease(string topic)
    {
        if (!SaveManager.player.isIntroProgrammingUnlocked && topic == "ITP")
        {
            cribManager.ShowMessage($"ITP is locked!");
            cribManager.UpdatePlayerInterface();
            return;
        }

        if (!SaveManager.player.isNumberSystemUnlocked && topic == "NS")
        {
            cribManager.ShowMessage($"NS is locked!");
            cribManager.UpdatePlayerInterface();
            return;
        }

        SaveManager.player.IncreaseStat(topic, 0.10f);
        cribManager.ShowMessage($"Your {topic} stat increased!");
        cribManager.UpdatePlayerInterface();
    }
}
    
