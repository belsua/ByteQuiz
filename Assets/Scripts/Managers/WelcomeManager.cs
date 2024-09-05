using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeManager : MonoBehaviour
{
    public GameObject Welcome;
    public GameObject OnScreenPanel;


    private void Start()
    {
        //ADDED
        if (SaveManager.player.needWelcome)
        {
            // Execute code if needWelcome is true
            Debug.Log("Welcome to the game! Let's get started.");
            Welcome.SetActive(true);
            OnScreenPanel.SetActive(false);

            SaveManager.player.needWelcome = false;
            if (Directory.Exists(SaveManager.saveFolder)) SaveManager.SavePlayer(SaveManager.player.slot);
        }
        else
        {
            // Execute code if needWelcome is false
            Debug.Log("Welcome back! Continue your progress.");
            Welcome.SetActive(false);
        }
    }
}
