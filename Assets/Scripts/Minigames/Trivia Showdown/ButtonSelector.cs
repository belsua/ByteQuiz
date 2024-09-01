using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.EventSystems;

public class ButtonSelector : MonoBehaviour
{
    Button targetButton;
    AudioSource audioSource;
    AudioClip clip;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
        audioSource = FindObjectOfType<AudioSource>();
        clip = Resources.Load<AudioClip>("Audio/Sound/ui-button");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PhotonView>().IsMine)
        {
            audioSource.PlayOneShot(clip);
            targetButton.Select();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PhotonView>().IsMine && !EventSystem.current.currentSelectedGameObject)
            targetButton.Select();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PhotonView>().IsMine)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
