using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    Vector2[] spawnPoints = { new(-7.5f, 2.75f), new(1.5f, 2.75f), new(10.5f, 2.75f), new(-16.5f, 2.75f) };

    private void Start()
    {
        SpawnPlayers();
    }

    [PunRPC]
    void InstantiatePlayer(int index)
    {
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[index], Quaternion.identity);
    }

    void SpawnPlayers()
    {
        int index = 0;
        if (PhotonNetwork.IsMasterClient)
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                photonView.RPC("InstantiatePlayer", player, index);
                index++;
            }
    }
}
