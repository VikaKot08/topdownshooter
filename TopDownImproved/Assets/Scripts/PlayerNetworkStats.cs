using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;

public class PlayerNetworkStats : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    public NetworkVariable<float> movementSpeed = new NetworkVariable<float>(3.5f);
    private NetworkVariable<float> gameTime = new NetworkVariable<float>(0f);

    private const float SPEED_INCREASE_INTERVAL = 60f; 
    private const float SPEED_INCREASE_AMOUNT = 0.5f;  
    private const float MAX_SPEED = 6f;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        health.OnValueChanged += OnHealthValueChanged;
        movementSpeed.OnValueChanged += OnSpeedValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (!IsOwner) return;

        health.OnValueChanged -= OnHealthValueChanged;
        movementSpeed.OnValueChanged -= OnSpeedValueChanged;
    }

    private void Update()
    {
        if (IsServer)
        {
            gameTime.Value += Time.deltaTime;

            
            if (gameTime.Value >= SPEED_INCREASE_INTERVAL)
            {
                gameTime.Value = 0f;
                IncreaseSpeedServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseSpeedServerRpc()
    {
        if (movementSpeed.Value < MAX_SPEED)
        {
            movementSpeed.Value += SPEED_INCREASE_AMOUNT;
        }
    }

    private void OnSpeedValueChanged(float oldValue, float newValue)
    {
        var playerMove = GetComponent<ClientPlayerMove>();
        if (playerMove != null)
        {
            playerMove.UpdateSpeed(newValue);
        }
    }
    public void ReceiveDamage(int damage)
    {
        ChangeHealthServerRpc(damage);
    }
    public void OnHealthValueChanged(int oldValue, int newValue)
    {
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeHealthServerRpc(int someValue)
    {
        health.Value += someValue;
        if (health.Value <= 0) 
        {
            SceneManager.LoadScene("Death");
        }
    }

}
