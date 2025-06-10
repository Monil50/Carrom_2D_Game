/*using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Button startGameButton;
    public TextMeshProUGUI roomInfoText; // Optional: To show room name, player count
    public Transform playerListContent; // Optional: Parent transform for player entries
    public GameObject playerListItemPrefab; // Optional: Prefab for each player in the list

    [Header("Game Settings")]
    public string gameSceneName = "GameScene"; // Make sure this matches your game scene's name

    private Dictionary<int, GameObject> playerListEntries; // For managing player list UI

    void Start()
    {
        // Ensure the button is disabled at the start for all clients
        startGameButton.interactable = false;
        UpdateRoomInfoUI();

        playerListEntries = new Dictionary<int, GameObject>();
    }

    // Called when a player successfully joins a room
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);

        // Enable the Start Game button ONLY for the Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable = true;
        }

        UpdateRoomInfoUI();
        UpdatePlayerList();
    }

    // Called when the Master Client changes (e.g., current Master leaves)
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master Client Switched to: " + newMasterClient.NickName);
        // Re-evaluate button state
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }

    // Called when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left the room.");
        UpdateRoomInfoUI();
        UpdatePlayerList();
        // If the remaining player is now Master, enable button if room is ready
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }

    // Called when a new player enters the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entered the room.");
        UpdateRoomInfoUI();
        UpdatePlayerList();
    }

    // --- Button Click Handler ---
    public void OnClickStartGame()
    {
        // This method should only be called by the Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            // Disable the button immediately to prevent multiple clicks
            startGameButton.interactable = false;

            // Option 1: Load Scene via PhotonNetwork.LoadLevel
            // This ensures all clients automatically load the same scene
            PhotonNetwork.LoadLevel(gameSceneName);

            // Option 2: Using an RPC (less common for scene loading, but good for custom logic)
            // PhotonNetwork.CurrentRoom.IsOpen = false; // Prevent new players from joining
            // PhotonNetwork.RPC("LoadGameSceneRPC", RpcTarget.All);
        }
        else
        {
            Debug.LogWarning("Only the Master Client can start the game.");
        }
    }

    // Optional: RPC for loading scene if you don't use PhotonNetwork.LoadLevel
    // [PunRPC]
    // public void LoadGameSceneRPC()
    // {
    //    PhotonNetwork.LoadLevel(gameSceneName);
    // }

    // --- UI Update Helpers ---
    private void UpdateRoomInfoUI()
    {
        if (roomInfoText != null && PhotonNetwork.InRoom)
        {
            roomInfoText.text = $"Room: {PhotonNetwork.CurrentRoom.Name} ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})";
        }
        else if (roomInfoText != null)
        {
            roomInfoText.text = "Not in a room.";
        }
    }

    private void UpdatePlayerList()
    {
        if (playerListItemPrefab == null || playerListContent == null) return;

        // Clear existing entries
        foreach (var entry in playerListEntries.Values)
        {
            Destroy(entry);
        }
        playerListEntries.Clear();

        // Add new entries for current players
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerListItemPrefab, playerListContent);
            TextMeshProUGUI playerNameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (playerNameText != null)
            {
                playerNameText.text = p.NickName + (p.IsMasterClient ? " (Master)" : "");
                playerNameText.color = p.IsMasterClient ? Color.yellow : Color.white; // Highlight master
            }
            playerListEntries.Add(p.ActorNumber, entry);
        }
    }
}*/

// 222222222222222222222222222222222222222222 * Modify


using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Button startGameButton;
    public TextMeshProUGUI roomInfoText; // Optional: To show room name, player count
    public Transform playerListContent; // Optional: Parent transform for player entries
    public GameObject playerListItemPrefab; // Optional: Prefab for each player in the list

    [Header("Game Settings")]
    public string gameSceneName = "GameScene"; // Make sure this matches your game scene's name

    private Dictionary<int, GameObject> playerListEntries; // For managing player list UI

    void Start()
    {
        // Ensure the button is disabled at the start for all clients
        startGameButton.interactable = false;
        UpdateRoomInfoUI();

        playerListEntries = new Dictionary<int, GameObject>();
    }

    // Called when a player successfully joins a room
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);

        // Enable the Start Game button ONLY for the Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable = true;
        }

        UpdateRoomInfoUI();
        UpdatePlayerList();
    }

    // Called when the Master Client changes (e.g., current Master leaves)
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master Client Switched to: " + newMasterClient.NickName);
        // Re-evaluate button state
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }

    // Called when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left the room.");
        UpdateRoomInfoUI();
        UpdatePlayerList();
        // If the remaining player is now Master, enable button if room is ready
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }

    // Called when a new player enters the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entered the room.");
        UpdateRoomInfoUI();
        UpdatePlayerList();
    }

    // --- Button Click Handler ---
    public void OnClickStartGame()
    {
        // This method should only be called by the Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            // Disable the button immediately to prevent multiple clicks
            startGameButton.interactable = false;

            // Option 1: Load Scene via PhotonNetwork.LoadLevel
            // This ensures all clients automatically load the same scene
            PhotonNetwork.LoadLevel(gameSceneName);

            // Option 2: Using an RPC (less common for scene loading, but good for custom logic)
            // PhotonNetwork.CurrentRoom.IsOpen = false; // Prevent new players from joining
            // PhotonNetwork.RPC("LoadGameSceneRPC", RpcTarget.All);
        }
        else
        {
            Debug.LogWarning("Only the Master Client can start the game.");
        }
    }

    // Optional: RPC for loading scene if you don't use PhotonNetwork.LoadLevel
    // [PunRPC]
    // public void LoadGameSceneRPC()
    // {
    //    PhotonNetwork.LoadLevel(gameSceneName);
    // }

    // --- UI Update Helpers ---
    private void UpdateRoomInfoUI()
    {
        if (roomInfoText != null && PhotonNetwork.InRoom)
        {
            roomInfoText.text = $"Room: {PhotonNetwork.CurrentRoom.Name} ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})";
        }
        else if (roomInfoText != null)
        {
            roomInfoText.text = "Not in a room.";
        }
    }

    private void UpdatePlayerList()
    {
        if (playerListItemPrefab == null || playerListContent == null) return;

        // Clear existing entries
        foreach (var entry in playerListEntries.Values)
        {
            Destroy(entry);
        }
        playerListEntries.Clear();

        // Add new entries for current players
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerListItemPrefab, playerListContent);
            TextMeshProUGUI playerNameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (playerNameText != null)
            {
                playerNameText.text = p.NickName + (p.IsMasterClient ? " (Master)" : "");
                playerNameText.color = p.IsMasterClient ? Color.yellow : Color.white; // Highlight master
            }
            playerListEntries.Add(p.ActorNumber, entry);
        }
    }
}