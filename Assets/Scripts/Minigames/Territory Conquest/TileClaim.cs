using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileClaim : MonoBehaviourPun
{
    public TerritoryConquest territoryConquest;
    public Tilemap tileMap;
    public Tile[] claimedTile;
    public Tile unclaimedTile;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!photonView.IsMine) return;

        Vector3 hitPosition = Vector3.zero;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            hitPosition.x = contact.point.x - 0.01f * contact.normal.x;
            hitPosition.y = contact.point.y - 0.01f * contact.normal.y;
            Vector3Int tilePosition = tileMap.WorldToCell(hitPosition);

            TileBase currentTile = tileMap.GetTile(tilePosition);
            if (currentTile == unclaimedTile)
            {
                tileMap.SetTile(tilePosition, claimedTile[territoryConquest.playerIndex]);
                photonView.RPC("ClaimTile", RpcTarget.All, tilePosition);
            }
        }
    }

    [PunRPC]
    public void ClaimTile(Vector3Int tilePosition)
    {
        tileMap.SetTile(tilePosition, claimedTile[territoryConquest.playerIndex]);
    }
}
