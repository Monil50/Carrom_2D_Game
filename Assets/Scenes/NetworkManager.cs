// NetworkManager.cs
using UnityEngine;
using Photon.Pun;       // Required for Photon PUN 2
using Photon.Realtime;  // Required for RoomOptions, Player etc.
using TMPro;            // Required for TextMeshPro UI

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TextMeshProUGUI connectionStatusText; // Text to show connection status
    public TextMeshProUGUI roomInfoText;         // Text to show room players
    public GameObject startButton;                // Button to start the game

    private const int MaxPlayersPerRoom = 2; // Fixed to 2 players for Carrom

    void Start()
    {
        // Ensure UI elements are linked
        if (connectionStatusText == null || roomInfoText == null || startButton == null)
        {
            Debug.LogError("UI references are not set in NetworkManager. Please assign them in the Inspector.");
            return;
        }

        connectionStatusText.text = "Connecting to Photon...";
        startButton.SetActive(false); // Hide start button initially

        // Connect to the Photon Cloud using the settings configured in the PhotonServerSettings
        // (which are set up during the PUN 2 import wizard).
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Attempting to connect to Master Server.");
    }

    // --- Photon Callbacks ---

    // Called when the client is connected to the Master Server and ready for matchmaking.
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster: Connected to Master Server.");
        connectionStatusText.text = "Connected. Joining Lobby...";

        // Join the default lobby to find or create rooms.
        PhotonNetwork.JoinLobby();
    }

    // Called when the client joined the lobby.
    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby: Joined default Lobby.");
        connectionStatusText.text = "Joined Lobby. Searching for Room...";

        // Try to join a random existing room. This is a common way to quickly match players.
        PhotonNetwork.JoinRandomRoom();
    }

    // Called when JoinRandomRoom failed (e.g., no rooms available or room is full).
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("OnJoinRandomFailed: {0} - {1}. No random room available, creating a new one.", returnCode, message);
        connectionStatusText.text = "Creating Room...";

        // If joining a random room failed, create a new room.
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = MaxPlayersPerRoom, // Set maximum players for this room
            IsVisible = true,              // Make room visible in lobby listings
            IsOpen = true                  // Allow players to join
        };
        // Generate a random room name (can be improved with more descriptive names)
        string roomName = "CarromRoom_" + Random.Range(1000, 9999);
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        Debug.Log("Creating Room: " + roomName);
    }

    // Called when the client successfully created a room.
    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom: Created Room: " + PhotonNetwork.CurrentRoom.Name);
        connectionStatusText.text = "Room Created: " + PhotonNetwork.CurrentRoom.Name;
        UpdateRoomInfoUI(); // Update UI immediately
    }

    // Called when the client successfully joined a room.
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom: Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        connectionStatusText.text = "Joined Room: " + PhotonNetwork.CurrentRoom.Name;
        UpdateRoomInfoUI(); // Update UI

        // If the room is full and this client is the Master Client, enable the start button.
        // Only the Master Client should initiate scene loading.
        if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom && PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            Debug.Log("Room is full. Master Client can start the game.");
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("You are the Master Client. Waiting for another player.");
        }
        else
        {
            Debug.Log("You are a connected client. Waiting for Master Client to start.");
        }
    }

    // Called when a remote player entered the room.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("OnPlayerEnteredRoom: Player {0} (ActorID: {1}) entered room. Current players: {2}",
            newPlayer.NickName, newPlayer.ActorNumber, PhotonNetwork.CurrentRoom.PlayerCount);
        UpdateRoomInfoUI();

        // If the room becomes full and this client is the Master Client, enable start button.
        if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom && PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            Debug.Log("Room is now full. Master Client can start the game.");
        }
    }

    // Called when a remote player left the room.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("OnPlayerLeftRoom: Player {0} (ActorID: {1}) left room. Current players: {2}",
            otherPlayer.NickName, otherPlayer.ActorNumber, PhotonNetwork.CurrentRoom.PlayerCount);
        UpdateRoomInfoUI();

        // If a player leaves and the room is no longer full, hide the start button for the Master Client.
        if (PhotonNetwork.CurrentRoom.PlayerCount < MaxPlayersPerRoom && PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(false);
            Debug.Log("A player left. Waiting for more players.");
        }
    }

    // Called when the client is disconnected from the Photon Cloud.
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogErrorFormat("OnDisconnected: Disconnected from Photon. Cause: {0}", cause);
        connectionStatusText.text = "Disconnected: " + cause.ToString() + ". Reconnecting...";
        // Optional: Reconnect or show an error screen. For simplicity, we just log.
    }

    // --- UI Update Method ---
    private void UpdateRoomInfoUI()
    {
        if (roomInfoText != null && PhotonNetwork.CurrentRoom != null)
        {
            roomInfoText.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
            if (PhotonNetwork.IsMasterClient)
            {
                roomInfoText.text += " (You are Master)";
            }
            else
            {
                roomInfoText.text += " (You are Client)";
            }
        }
    }

    // --- Game Start Method (called by UI Button) ---
    public void StartGame()
    {
        // Only the Master Client can load the scene to ensure everyone loads at the same time.
        if (PhotonNetwork.IsMasterClient)
        {
            // Check if room is actually full before starting
            if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom)
            {
                // PhotonNetwork.LoadLevel loads the scene for all clients in the room.
                // Make sure "GameScene" is added to your Build Settings (File > Build Settings).
                PhotonNetwork.LoadLevel("GameScene");
                Debug.Log("Master Client initiating scene load to GameScene.");
            }
            else
            {
                Debug.LogWarning("Cannot start game: Not enough players in the room.");
            }
        }
        else
        {
            Debug.LogWarning("Only the Master Client can start the game.");
        }
    }
}