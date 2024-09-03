using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeManager : MonoBehaviour
{
    public GameObject Welcome;
    public GameObject Joystick;
    public GameObject InteractBtn;
    public GameObject Settings;
    public GameObject WelcomeA;
    public GameObject WelcomeB;
    public GameObject WelcomeC;
    public GameObject WelcomeD;
    public GameObject WelcomeE;
    public GameObject WelcomeF;
    public GameObject WelcomeG;
    public GameObject HelpButton;


    private void Start()
    {
        //ADDED
        if (SaveManager.instance.needWelcome)
        {
            // Execute code if needWelcome is true
            ShowWelcomeMessage();
            Welcome.SetActive(true);
            Joystick.SetActive(false);
            InteractBtn.SetActive(false);
            Settings.SetActive(false);
            HelpButton.SetActive(false);
            WelcomeA.SetActive(true);
            WelcomeB.SetActive(false);
            WelcomeC.SetActive(false);
            WelcomeD.SetActive(false);
            WelcomeE.SetActive(false);
            WelcomeF.SetActive(false);
            WelcomeG.SetActive(false);
        }
        else
        {
            // Execute code if needWelcome is false
            ProceedWithoutWelcome();
            Welcome.SetActive(false);
        }
    }

    //ADDED
    void ShowWelcomeMessage()
    {
        // Display a welcome message or perform any required initialization
        Debug.Log("Welcome to the game! Let's get started.");
        // Implement other welcome-related actions here
    }

    void ProceedWithoutWelcome()
    {
        // Proceed with normal game initialization
        Debug.Log("Welcome back! Continue your progress.");
        // Implement other actions for returning players here
    }
}
