using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeManager : MonoBehaviour
{
    public GameObject Welcome;
    public GameObject OnScreenPanel;
    public AudioSource audioSource;
    public AudioClip WelcomeA;
    public AudioClip OtherClip;


    private void Start()
    {
        //ADDED
        if (SaveManager.player.needWelcome)
        {
            // Execute code if needWelcome is true
            Debug.Log("Welcome to the game! Let's get started.");
            Welcome.SetActive(true);
            OnScreenPanel.SetActive(false);

            audioSource.PlayOneShot(WelcomeA);


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


    // Function to play an audio clip
    public void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Function to stop a specific audio clip
    public void StopSpecificAudioClip(AudioClip clipToStop)
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == clipToStop)
        {
            audioSource.Stop();
        }
    }

}
