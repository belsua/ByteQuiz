using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelector : MonoBehaviour
{
    Button targetButton;
    AudioSource audioSource;
    AudioClip clip;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
        audioSource = FindAnyObjectByType<AudioSource>();
        clip = Resources.Load<AudioClip>("Audio/ui-button");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log($"{other.name} clicked {targetButton.gameObject.name}");
            audioSource.PlayOneShot(clip);
            //targetButton.Select();
            EventSystem.current.SetSelectedGameObject(targetButton.gameObject);
            Debug.Log($"Selected: {EventSystem.current.currentSelectedGameObject.name}, correct: {EventSystem.current.currentSelectedGameObject.GetComponent<Answers>().isCorrect}");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log($"{other.name} clicked {targetButton.gameObject.name}");
            if (EventSystem.current.currentSelectedGameObject == false)
                EventSystem.current.SetSelectedGameObject(targetButton.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log($"{other.name} exited {targetButton.gameObject.name}");
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
