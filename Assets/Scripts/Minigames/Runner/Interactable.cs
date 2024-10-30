using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    Runner runner;
    Button interactButton;

    protected virtual void Start()
    {
        runner = FindAnyObjectByType<Runner>();
        interactButton = GameObject.Find("InteractButton").GetComponent<Button>();
        interactButton.onClick.AddListener(() => runner.ToggleQuiz(true));
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        PhotonView photonView = collision.gameObject.GetComponent<PhotonView>();  
        if (collision.gameObject.CompareTag("Player") && photonView.IsMine)
        {
            interactButton.interactable = true;
            TeleportLocation.collidedObject = gameObject;
            Debug.Log($"{photonView.Owner.NickName} ({photonView.ViewID}) collided with: {gameObject.name}");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        interactButton.interactable = false;
    }
}
