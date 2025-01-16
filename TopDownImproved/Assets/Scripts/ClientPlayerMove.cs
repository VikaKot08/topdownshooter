using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField]
    private ThirdPersonController playerController;
    [SerializeField]
    private PlayerInput playerInput;
    float moveSpeed;


    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<ThirdPersonController>();
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        var stats = GetComponent<PlayerNetworkStats>();
        if (stats != null)
        {
            moveSpeed = stats.movementSpeed.Value;
        }
        else
        {
            moveSpeed = 3.5f;
        }

        SetComponentsEnabled(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            enabled = false;
            SetComponentsEnabled(false);
            return;
        }

        enabled = true;
        SetComponentsEnabled(true);
    }

    private void SetComponentsEnabled(bool enabled)
    {
        if (playerInput != null) playerInput.enabled = enabled;
        if (playerController != null) playerController.enabled = enabled;
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 input = playerController.input.move;
        Vector3 moveInput = new Vector3(input.x, input.y, 0);

        if (moveInput.magnitude > 0.1f)
        {
            MovePlayerServerRpc(moveInput);
        }
    }

    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 moveInput)
    {
        Vector3 inputDirection = new Vector3(moveInput.x, moveInput.y, 0.0f).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 moveVector = inputDirection * moveSpeed * Time.deltaTime;
            Vector3 potentialPosition = transform.position + moveVector;

            if (IsWithinScreenBounds(potentialPosition))
            {
                transform.position = potentialPosition;
            }
        }
        SyncPositionClientRpc(transform.position);
    }

    private bool IsWithinScreenBounds(Vector3 position)
    {
        Vector3 minBounds = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 maxBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));

        return position.x >= minBounds.x && position.x <= maxBounds.x &&
               position.y >= minBounds.y && position.y <= maxBounds.y;
    }

    [ClientRpc]
    private void SyncPositionClientRpc(Vector3 position)
    {
        if (IsOwner) return;
        transform.position = position;
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}