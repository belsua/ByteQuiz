using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static void PlaySound(AudioClip clip)
    {
        GameObject tempGO = new("TempAudio");
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
        Destroy(tempGO, clip.length);
    }
}
