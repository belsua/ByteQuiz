using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class FinishLine : MonoBehaviourPunCallbacks
{
    //[SerializeField] Transform teleportLocation;

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        Teleport(other.gameObject);
    //    }
    //}

    //private void Teleport(GameObject player)
    //{
    //    if (PhotonNetwork.IsConnected && !player.GetComponent<PhotonView>().IsMine)
    //    {
    //        return;
    //    }

    //    player.transform.position = teleportLocation.position;
    //}

    //public override void OnDisconnected(DisconnectCause cause)
    //{
    //    Debug.LogWarning("Disconnected from Photon with cause: " + cause);
    //    SceneManager.LoadScene("Lobby");
    //}
}
