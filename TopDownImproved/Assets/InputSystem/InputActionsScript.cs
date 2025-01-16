using Unity.Netcode;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif

public class InputActionsScript : NetworkBehaviour
{
    public Vector2 move;

    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;
    public float singleShotReloadTime = 0.5f;
    public float multipleShotReloadTime = 1.5f;
    public float diagonalAngle = 30f;
    public float projectileOffset = 1f;
    public bool debugShooting = true;

    private Transform playerTransform;

    private ulong playerNumber;
    private bool canShootSingle = true;
    private bool canShootMultiple = true;

    private bool chatActive = true;

    private void Awake()
    {
        playerTransform = transform;
    }

    public void changeChat(bool state)
    {
        chatActive = state;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerNumber = OwnerClientId;
    }


    public void OnMove(InputValue value)
    {
        if (!chatActive)
        {
            MoveInput(value.Get<Vector2>());
        }
    }

    public void OnShootOne(InputValue value)
    {
        if (!IsOwner || !canShootSingle || chatActive) return;
        ShootOneInput();
    }

    public void OnShootMultiple(InputValue value)
    {
        if (!IsOwner || !canShootMultiple || chatActive) return;
        ShootMultipleInput();
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void ShootOneInput()
    {
        if (!canShootSingle) return;

        Vector3 spawnPosition = playerTransform.position;
        Vector3 shootDirection = playerNumber == 0 ? Vector3.up : Vector3.down;

        ShootSingleServerRpc(spawnPosition, shootDirection);
        StartCoroutine(SingleShotReloadRoutine());
    }

    public void ShootMultipleInput()
    {
        if (!canShootMultiple) return;
        Vector3 spawnPosition = playerTransform.position;
        Vector3 baseDirection = playerNumber == 0 ? Vector3.up : Vector3.down;

        ShootMultipleServerRpc(spawnPosition, baseDirection);
        StartCoroutine(MultipleShotReloadRoutine());
    }

    private System.Collections.IEnumerator SingleShotReloadRoutine()
    {
        canShootSingle = false;
        yield return new WaitForSeconds(singleShotReloadTime);
        canShootSingle = true;
    }

    private System.Collections.IEnumerator MultipleShotReloadRoutine()
    {
        canShootMultiple = false;
        yield return new WaitForSeconds(multipleShotReloadTime);
        canShootMultiple = true;
    }

    [ServerRpc]
    private void ShootSingleServerRpc(Vector3 spawnPosition, Vector3 direction)
    {
        Vector3 offsetPosition = spawnPosition + (direction * projectileOffset);
        SpawnProjectile(offsetPosition, direction);
    }

    [ServerRpc]
    private void ShootMultipleServerRpc(Vector3 spawnPosition, Vector3 baseDirection)
    {
        Vector3 leftDirection = Quaternion.Euler(0, 0, diagonalAngle) * baseDirection;
        Vector3 rightDirection = Quaternion.Euler(0, 0, -diagonalAngle) * baseDirection;

        Vector3 perpendicularOffset = Vector3.Cross(baseDirection, Vector3.forward).normalized * 1.5f;

        Vector3 centerPosition = spawnPosition + (baseDirection * projectileOffset);
        Vector3 leftPosition = centerPosition - perpendicularOffset;
        Vector3 rightPosition = centerPosition + perpendicularOffset; 

        SpawnProjectile(leftPosition, leftDirection);
        SpawnProjectile(centerPosition, baseDirection);
        SpawnProjectile(rightPosition, rightDirection);
    }

    private void SpawnProjectile(Vector3 spawnPosition, Vector3 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = projectile.GetComponent<NetworkObject>();
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        if (networkObject != null)
        {
            networkObject.Spawn();
            StartCoroutine(DespawnProjectileAfterDelay(networkObject));
        }
    }

    private System.Collections.IEnumerator DespawnProjectileAfterDelay(NetworkObject projectile)
    {
        yield return new WaitForSeconds(10f);

        if (projectile != null && projectile.IsSpawned)
        {
            projectile.Despawn(true);
        }
    }
}