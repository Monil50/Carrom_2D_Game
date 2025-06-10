// PocketHandler.cs
using Photon.Pun; // Required for Photon
using UnityEngine;

public class PocketHandler : MonoBehaviour
{
    private GameManager gameManager; // Reference to the GameManager in the scene

    void Start()
    {
        // Find the GameManager in the scene. Make sure GameManager is active.
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("PocketHandler: GameManager not found in scene! Make sure it's present and active.");
        }
    }

    // This method is called by Unity when another Collider2D enters this trigger collider.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Get the PhotonView from the colliding object.
        PhotonView otherPv = other.GetComponent<PhotonView>();

        // We only care about networked objects that have a PhotonView.
        if (otherPv != null)
        {
            // IMPORTANT: Only the Master Client should process pocketing events to avoid conflicts
            // and ensure consistent game state across all clients.
            // Other clients will see the outcome through Photon's synchronization and RPCs.
            if (PhotonNetwork.IsMasterClient)
            {
                CoinProperties coin = other.GetComponent<CoinProperties>();
                StrikerController striker = other.GetComponent<StrikerController>();

                if (coin != null)
                {
                    // If a coin is pocketed, send an RPC to all clients to update scores and destroy the coin.
                    // We pass the PhotonView ID so all clients know which specific coin was pocketed.
                    gameManager.pv.RPC("Pun_CoinPotted", RpcTarget.All, otherPv.ViewID);
                }
                else if (striker != null)
                {
                    // If the striker is pocketed (foul), send an RPC to all clients.
                    // This is more complex, as you might need to check if it's the current player's striker.
                    // For now, assume any striker in pocket is a foul.
                    gameManager.pv.RPC("Pun_StrikerPotted", RpcTarget.All);
                }
            }
        }
    }
}