// StrikerController.cs
using UnityEngine;
using Photon.Pun; // Required for Photon

public class StrikerController : MonoBehaviourPun
{
    [Header("Striker Settings")]
    public float maxPower = 10f;        // Max force of the shot
    public float minPower = 1f;         // Min force to consider a shot
    public float drawBackLimit = 2f;    // How far back the player can drag for power

    private Rigidbody2D rb;
    private Vector2 startDragPos;
    private Vector2 currentDragPos;
    private bool isDragging = false;
    private Vector2 initialStrikerPosition; // Stores the starting position for reset
    private GameManager gameManager;        // Reference to the GameManager

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Find the GameManager in the scene.
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null) Debug.LogError("StrikerController: GameManager not found in scene!");

        // Store the initial position this striker spawned at.
        // This will be either P1 or P2 spawn point depending on which player instantiated it.
        initialStrikerPosition = transform.position;
    }

    void Update()
    {
        // CRUCIAL MULTIPLAYER LOGIC:
        // 1. photonView.IsMine: Ensures only the local client controls its own striker.
        // 2. gameManager.IsMyTurn(): Checks if it's currently this local player's turn.
        // (IsMyTurn will be a helper method in GameManager, defined below)
        if (photonView.IsMine && gameManager.IsMyTurn())
        {
            // Allow the player to reset the striker to its base position (e.g., if it moved accidentally)
            // This could also be a UI button.
            if (Input.GetKeyDown(KeyCode.R) && rb.velocity.magnitude < 0.1f)
            {
                ResetStrikerPosition();
            }

            // --- Striker Positioning and Shooting Logic ---
            if (Input.GetMouseButtonDown(0) && rb.velocity.magnitude < 0.1f) // Only if not moving
            {
                // Get mouse position in world coordinates
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Check if the mouse clicked on this specific striker's collider.
                // This is a basic check; for precise Carrom, you'd constrain to a baseline.
                if (GetComponent<Collider2D>().OverlapPoint(mouseWorldPos))
                {
                    isDragging = true;
                    startDragPos = mouseWorldPos;
                    // Stop any existing velocity to prevent interference with new shot.
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }

            if (isDragging)
            {
                currentDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // Calculate direction and distance for aiming.
                // For a 2D game, you might draw a line to indicate aiming and power.
                // Draw aiming line in editor for debugging/visual feedback.
                //Debug.DrawLine(transform.position, transform.position + (startDragPos - currentDragPos).normalized * 2f, Color.red);
            }

            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                // Calculate the vector from current mouse position back to start of drag.
                Vector2 dragVector = startDragPos - currentDragPos;
                // Calculate power based on drag distance, clamped by drawBackLimit.
                float power = Mathf.Clamp(dragVector.magnitude, 0, drawBackLimit) / drawBackLimit * maxPower;
                // Direction of the shot.
                Vector2 shootDirection = dragVector.normalized;

                if (power > minPower) // Only shoot if enough power was applied
                {
                    // Apply force as an Impulse for an instant push.
                    // PhotonRigidbody2DView will automatically synchronize this force across the network.
                    rb.AddForce(shootDirection * power, ForceMode2D.Impulse);

                    // Notify the GameManager that a shot has been fired.
                    // The GameManager will then wait for pieces to stop and manage the turn change.
                    gameManager.StartShotEvaluationMultiplayer();
                }
            }
        }
        else
        {
            // If it's not our turn or not our striker, ensure it's kinematic to prevent accidental movement.
            // This is handled by GameManager's SetPlayerTurn RPC.
            // rb.isKinematic = true; // No, let PhotonRigidbody2DView handle this on non-owners.
        }
    }

    // Called to reset the striker to its initial spawn position.
    public void ResetStrikerPosition()
    {
        // Only the owner can directly manipulate its transform/rigidbody.
        if (photonView.IsMine)
        {
            transform.position = initialStrikerPosition;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}