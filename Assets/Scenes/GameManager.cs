// GameManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;       // Required for Photon PUN 2
using Photon.Realtime;  // Required for Player class
using TMPro;            // Required for TextMeshPro UI

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Game Piece Prefabs (Needs PhotonView)")]
    public GameObject whiteCoinPrefab;
    public GameObject blackCoinPrefab;
    public GameObject queenPrefab;
    public GameObject strikerPrefab;

    [Header("Spawn Points")]
    public Transform[] whiteCoinSpawnPoints;
    public Transform[] blackCoinSpawnPoints;
    public Transform queenSpawnPoint;
    public Transform strikerSpawnPointP1; // Player 1's striker spawn
    public Transform strikerSpawnPointP2; // Player 2's striker spawn

    [Header("Game Rules & Settings")]
    public LayerMask pocketLayer; // Layer for pocket triggers
    public float minPieceStopVelocity = 0.1f; // Velocity threshold for pieces to be considered stopped

    [Header("UI References")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI myPlayerIdText; // Helps in debugging which player you are

    // Game State Variables (synchronized implicitly or explicitly via RPCs)
    public int player1Score = 0;
    public int player2Score = 0;

    public enum PlayerTurn { None, Player1, Player2 } // None for initial state/game end
    public PlayerTurn currentPlayerTurn = PlayerTurn.None;

    // References to Photon Player objects (important for mapping turns to actual players)
    private Photon.Realtime.Player player1; // Corresponds to PlayerTurn.Player1
    private Photon.Realtime.Player player2; // Corresponds to PlayerTurn.Player2

    private StrikerController localPlayerStriker; // The striker controlled by this client
    private bool queenPotted = false;
    private bool wasLastShotAFoul = false;
    private bool hasCoveredQueen = false; // Carrom specific: if queen was covered after potting

    // PhotonView reference for sending/receiving RPCs
    public PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>(); // Get the PhotonView attached to this GameObject
        if (pv == null) Debug.LogError("GameManager: PhotonView component missing!");
    }

    void Start()
    {
        // Ensure Photon is connected before proceeding (should be from LobbyScene)
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Photon is not connected or ready! Please start from LobbyScene.");
            // Optionally, return to lobby scene or handle error
            PhotonNetwork.LoadLevel("LobbyScene");
            return;
        }

        // Assign player references based on the Photon room's player list.
        // PlayerList[0] is typically the Master Client, but it's more reliable
        // to assign them once the room is established.
        InitializePlayers();

        // Only the Master Client is responsible for instantiating the game pieces
        // to ensure consistent initial state across all clients. Photon will sync them.
        if (PhotonNetwork.IsMasterClient)
        {
            SetupBoard();
        }

        // Each player spawns their own striker locally using PhotonNetwork.Instantiate.
        // Ownership will automatically be assigned to the instantiating client.
        SpawnLocalPlayerStriker();

        UpdateUI();

        // Master Client initiates the game sequence after all players have loaded the scene.
        // Use a slight delay to ensure all clients have received spawned objects.
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(StartGameWithDelay(1.0f)); // Delay for 1 second
        }
    }

    IEnumerator StartGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartGameSequence();
    }

    // Assigns player1 and player2 references based on the current room's player list.
    void InitializePlayers()
    {
        if (PhotonNetwork.PlayerList.Length >= 1)
        {
            player1 = PhotonNetwork.PlayerList[0];
            if (PhotonNetwork.PlayerList.Length >= 2)
            {
                player2 = PhotonNetwork.PlayerList[1];
            }
        }

        // Display which player the local client is for debugging/UI.
        if (PhotonNetwork.LocalPlayer == player1)
        {
            myPlayerIdText.text = "You are Player 1 (" + PhotonNetwork.LocalPlayer.NickName + ")";
        }
        else if (PhotonNetwork.LocalPlayer == player2)
        {
            myPlayerIdText.text = "You are Player 2 (" + PhotonNetwork.LocalPlayer.NickName + ")";
        }
        else
        {
            myPlayerIdText.text = "Observer (Error)"; // Should not happen in 2-player game
            Debug.LogWarning("Local player is neither Player 1 nor Player 2.");
        }
    }

    // Master Client calls this to instantiate all the Carrom coins and Queen.
    void SetupBoard()
    {
        // Instantiate coins using PhotonNetwork.Instantiate. This spawns them on all clients
        // and assigns ownership to the instantiating client (Master Client in this case).
        // Ensure whiteCoinPrefab, blackCoinPrefab, queenPrefab have PhotonView components.
        foreach (Transform t in whiteCoinSpawnPoints)
        {
            PhotonNetwork.Instantiate(whiteCoinPrefab.name, t.position, Quaternion.identity);
        }
        foreach (Transform t in blackCoinSpawnPoints)
        {
            PhotonNetwork.Instantiate(blackCoinPrefab.name, t.position, Quaternion.identity);
        }
        PhotonNetwork.Instantiate(queenPrefab.name, queenSpawnPoint.position, Quaternion.identity);

        Debug.Log("Master Client: Board setup complete.");
    }

    // Each client spawns their own striker.
    void SpawnLocalPlayerStriker()
    {
        Vector3 spawnPos;
        if (PhotonNetwork.LocalPlayer == player1)
        {
            spawnPos = strikerSpawnPointP1.position;
        }
        else if (PhotonNetwork.LocalPlayer == player2)
        {
            spawnPos = strikerSpawnPointP2.position;
        }
        else
        {
            Debug.LogError("Error: Local Player not recognized for striker spawn.");
            return;
        }

        // Instantiate the striker. Its ownership will be local to this client.
        // Ensure strikerPrefab has PhotonView, PhotonRigidbody2DView, PhotonTransformViewClassic.
        localPlayerStriker = PhotonNetwork.Instantiate(strikerPrefab.name, spawnPos, Quaternion.identity).GetComponent<StrikerController>();

        // Initially disable striker control and physics for all players until their turn.
        // The SetPlayerTurn RPC will enable it.
        localPlayerStriker.enabled = false;
        localPlayerStriker.GetComponent<Rigidbody2D>().isKinematic = true; // Freeze it
        Debug.Log("Spawned local player striker: " + localPlayerStriker.name);
    }

    // Called by the Master Client to start the game and set the first turn.
    public void StartGameSequence()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Master client decides who starts first (e.g., Player 1)
            pv.RPC("SetPlayerTurn", RpcTarget.All, player1.ActorNumber);
            Debug.Log("Master Client: Starting game sequence, Player 1 turn.");
        }
    }

    // RPC: Sets the current player's turn on all clients.
    [PunRPC]
    public void SetPlayerTurn(int playerActorNumber)
    {
        if (player1 != null && playerActorNumber == player1.ActorNumber)
        {
            currentPlayerTurn = PlayerTurn.Player1;
            Debug.LogFormat("Turn set to Player 1 (Actor: {0})", playerActorNumber);
        }
        else if (player2 != null && playerActorNumber == player2.ActorNumber)
        {
            currentPlayerTurn = PlayerTurn.Player2;
            Debug.LogFormat("Turn set to Player 2 (Actor: {0})", playerActorNumber);
        }
        else
        {
            currentPlayerTurn = PlayerTurn.None;
            Debug.LogError("Invalid player actor number for SetPlayerTurn: " + playerActorNumber);
        }

        // Enable/disable striker control based on whose turn it is
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerActorNumber)
        {
            localPlayerStriker.enabled = true; // Enable input for your own striker
            localPlayerStriker.GetComponent<Rigidbody2D>().isKinematic = false; // Allow physics
            localPlayerStriker.ResetStrikerPosition(); // Move striker back to your line
            Debug.Log("It's your turn!");
        }
        else
        {
            if (localPlayerStriker != null)
            {
                localPlayerStriker.enabled = false; // Disable input for opponent's striker
                localPlayerStriker.GetComponent<Rigidbody2D>().isKinematic = true; // Freeze opponent's striker
                Debug.Log("It's opponent's turn. Your striker is disabled.");
            }
        }
        UpdateUI(); // Update UI for the new turn
    }

    // Helper: Checks if it's the local player's turn.
    public bool IsMyTurn()
    {
        if (PhotonNetwork.LocalPlayer == player1 && currentPlayerTurn == PlayerTurn.Player1) return true;
        if (PhotonNetwork.LocalPlayer == player2 && currentPlayerTurn == PlayerTurn.Player2) return true;
        return false;
    }

    // RPC: Called by PocketHandler (Master Client) to inform all clients about a potted coin.
    [PunRPC]
    public void Pun_CoinPotted(int viewID)
    {
        PhotonView pottedCoinPv = PhotonView.Find(viewID);
        if (pottedCoinPv == null)
        {
            Debug.LogWarning("Pun_CoinPotted: Potted coin PhotonView not found for ID: " + viewID);
            return;
        }

        GameObject pottedCoin = pottedCoinPv.gameObject;
        string coinTag = pottedCoin.tag; // Ensure your prefabs have tags: "WhiteCoin", "BlackCoin", "Queen"
        Debug.LogFormat("RPC: Coin {0} (ViewID: {1}) was pocketed!", pottedCoin.name, viewID);

        // Destroy the coin on all clients. Since Master Client initiated, it's synchronized.
        PhotonNetwork.Destroy(pottedCoin);

        // Score update logic (Master Client decides, then synchronizes via RPC)
        if (PhotonNetwork.IsMasterClient)
        {
            // Basic scoring logic (needs to be more complex for full Carrom rules)
            if (coinTag == "WhiteCoin")
            {
                if (currentPlayerTurn == PlayerTurn.Player1) player1Score++;
                else player2Score--; // Example penalty for potting opponent's coin on your turn
            }
            else if (coinTag == "BlackCoin")
            {
                if (currentPlayerTurn == PlayerTurn.Player2) player2Score++;
                else player1Score--; // Example penalty
            }
            else if (coinTag == "Queen")
            {
                queenPotted = true;
                // Add queen-specific scoring rules here (e.g., need to cover it)
            }
            // Send updated scores to all clients.
            pv.RPC("UpdateScoresRPC", RpcTarget.All, player1Score, player2Score);
        }
    }

    // RPC: Called by PocketHandler (Master Client) to inform all clients about a potted striker.
    [PunRPC]
    public void Pun_StrikerPotted()
    {
        wasLastShotAFoul = true; // Mark as foul for turn evaluation
        Debug.Log("RPC: Striker pocketed! Foul!");

        if (PhotonNetwork.IsMasterClient)
        {
            // Example penalty for striker pocketing
            if (currentPlayerTurn == PlayerTurn.Player1) player1Score--;
            else player2Score--;
            pv.RPC("UpdateScoresRPC", RpcTarget.All, player1Score, player2Score); // Sync scores
        }
        // Striker is reset on local client in StrikerController's ResetStrikerPosition()
        // and synchronized by PhotonRigidbody2DView.
        localPlayerStriker.ResetStrikerPosition();
    }

    // RPC: Synchronizes scores across all clients.
    [PunRPC]
    void UpdateScoresRPC(int p1Score, int p2Score)
    {
        player1Score = p1Score;
        player2Score = p2Score;
        UpdateUI();
    }

    // Called by StrikerController after a shot is fired.
    public void StartShotEvaluationMultiplayer()
    {
        // This method should be called by the client that fired the shot.
        // It then starts a Coroutine to wait for all pieces to stop.
        StartCoroutine(EvaluateShotRoutineMultiplayer());
    }

    private IEnumerator EvaluateShotRoutineMultiplayer()
    {
        // Wait until all networked pieces (coins and strikers) have come to a complete stop.
        // This uses a loop to check velocity of all Rigidbody2D objects with PhotonView.
        yield return new WaitUntil(AllPiecesStopped);

        // IMPORTANT: Only the Master Client should decide the next turn and handle complex end-of-turn logic
        // to prevent conflicts and ensure consistent game flow.
        if (PhotonNetwork.IsMasterClient)
        {
            // Implement complex Carrom rules here:
            // - Check if any coins were pocketed.
            // - Check for foul conditions (striker pocketed, no coin pocketed, opponent's coin pocketed alone).
            // - Handle Queen covering rules.
            // - Determine if any coins need to be returned to the board.

            // For simplicity, let's just advance the turn to the other player.
            int nextPlayerActorNumber;
            if (currentPlayerTurn == PlayerTurn.Player1)
            {
                nextPlayerActorNumber = player2.ActorNumber;
            }
            else
            {
                nextPlayerActorNumber = player1.ActorNumber;
            }

            // Send RPC to all clients to advance the turn.
            pv.RPC("AdvanceTurnRPC", RpcTarget.All, nextPlayerActorNumber);
        }
    }

    // Checks if all game pieces (coins + local striker) have stopped moving.
    bool AllPiecesStopped()
    {
        // Find all Rigidbody2D components that are also networked (have a PhotonView).
        Rigidbody2D[] allNetworkedRBs = FindObjectsOfType<Rigidbody2D>();

        foreach (Rigidbody2D rb in allNetworkedRBs)
        {
            // Exclude the board's collider if it has a Rigidbody2D (e.g., if kinematic)
            // Or make sure only relevant game pieces have Rigidbody2Ds.
            if (rb.GetComponent<PhotonView>() != null && rb.velocity.magnitude > minPieceStopVelocity)
            {
                return false;
            }
        }
        return true;
    }

    // RPC: Advances the turn on all clients.
    [PunRPC]
    public void AdvanceTurnRPC(int nextPlayerActorNumber)
    {
        SetPlayerTurn(nextPlayerActorNumber); // Call the turn setting logic
        wasLastShotAFoul = false; // Reset foul status for next turn
        // Add more end-of-turn resets/cleanup here
    }

    // Updates the score and turn display in the UI.
    void UpdateUI()
    {
        if (player1ScoreText)
        {
            player1ScoreText.text = "P1 (" + (player1 != null ? player1.NickName : "Waiting") + "): " + player1Score;
        }
        if (player2ScoreText)
        {
            player2ScoreText.text = "P2 (" + (player2 != null ? player2.NickName : "Waiting") + "): " + player2Score;
        }
        if (turnText)
        {
            if (currentPlayerTurn == PlayerTurn.Player1 && player1 != null)
            {
                turnText.text = "Turn: " + player1.NickName;
            }
            else if (currentPlayerTurn == PlayerTurn.Player2 && player2 != null)
            {
                turnText.text = "Turn: " + player2.NickName;
            }
            else
            {
                turnText.text = "Waiting for Game Start...";
            }
        }
    }

    // You might also want to handle what happens if a player leaves during the game
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.LogFormat("Player {0} left the game room.", otherPlayer.NickName);
        // Handle game end, notify remaining player, return to lobby, etc.
        // For example:
        // pv.RPC("EndGameRPC", RpcTarget.All, "Opponent left the game!");
        PhotonNetwork.LoadLevel("LobbyScene"); // For simplicity, go back to lobby
    }
}