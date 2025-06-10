using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonRoomJoiner : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();  // Step 1: Connect to Photon server
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");

        PhotonNetwork.JoinRandomRoom();  // Step 2: Try to join any available random room
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No random room found, creating a new one.");

        // Step 3: Create a new room if none exists
        string roomName = "Room_" + Random.Range(1000, 9999);  // Random name
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 4 };  // Max 4 players

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        // Now you are in the room and can start game logic
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon: " + cause.ToString());
    }
}
