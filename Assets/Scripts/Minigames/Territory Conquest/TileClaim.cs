using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileClaim : MonoBehaviourPun
{
    public TerritoryConquest territoryConquest;
    public Tilemap tileMap;
    public Tile[] claimedTile;
    Vector3 hitPosition;
    Vector3Int tilePosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.GetComponent<PhotonView>().IsMine) return;

        hitPosition = collision.transform.position;
        tilePosition = tileMap.WorldToCell(hitPosition);
        TileBase currentTile = tileMap.GetTile(tilePosition);

        if (currentTile != claimedTile[territoryConquest.playerIndex])
        {
            territoryConquest.FreezePlayer();
            territoryConquest.quizPanel.SetActive(true);
        }
    }

    public void ClaimTile()
    {
        tileMap.SetTile(tilePosition, claimedTile[territoryConquest.playerIndex]);

        photonView.RPC("ClaimTileRPC", RpcTarget.All, tilePosition.x, tilePosition.y, tilePosition.z, territoryConquest.playerIndex);
    }

    [PunRPC]
    public void ClaimTileRPC(int x, int y, int z, int playerIndex)
    {
        Vector3Int tilePosition = new(x, y, z);
        tileMap.SetTile(tilePosition, claimedTile[playerIndex]);

        int count = 0;
        foreach (Vector3Int cellPosition in tileMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tileMap.GetTile(cellPosition);
            if (tile == claimedTile[territoryConquest.playerIndex]) count++;
        }

        territoryConquest.score = count;
        territoryConquest.scoreText.text = $"Score: {count}";
        territoryConquest.ChangeScoreList(PhotonNetwork.LocalPlayer.NickName, count);
    }
}
