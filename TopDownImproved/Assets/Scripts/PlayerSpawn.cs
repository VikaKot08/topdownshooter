using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerOnePrefab;
    [SerializeField] private GameObject playerTwoPrefab;
    private Vector3 playerOneSpawnPoint = new Vector3(0, -4.5f, 0);
    private Vector3 playerTwoSpawnPoint = new Vector3(0, 4.5f, 0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayerForClient;
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        int playerIndex = NetworkManager.Singleton.ConnectedClientsIds.Count - 1;


        GameObject prefabToSpawn = playerIndex == 0 ? playerOnePrefab : playerTwoPrefab;

        Vector3 spawnPosition = playerIndex == 0 ? playerOneSpawnPoint : playerTwoSpawnPoint;

        Quaternion spawnRotation = playerIndex == 0
            ? Quaternion.identity
            : Quaternion.Euler(0, 0, 180f);

        GameObject playerInstance = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayerForClient;
    }
}