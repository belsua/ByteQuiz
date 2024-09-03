using Photon.Pun;
using UnityEngine;
using TMPro;

public class TerritoryConquest : Minigame
{
    [Header("Minigame Variables")]
    public Vector2[] spawnPoints = new Vector2[4];
    public TMP_Text turnText;
    internal int playerIndex;

    public override void SpawnPlayers()
    {
        playerPrefab.GetComponent<SpriteRenderer>().sortingOrder = playerSpriteOrder;
        playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[playerIndex], Quaternion.identity);
    }
}
