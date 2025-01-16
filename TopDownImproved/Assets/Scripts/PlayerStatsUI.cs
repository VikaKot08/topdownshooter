using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : NetworkBehaviour
{
    public Slider healthSlider;
    private PlayerNetworkStats playerNetworkStats; 

    public void Initialize(PlayerNetworkStats playerStats)
    {
        playerNetworkStats = playerStats;
        healthSlider.maxValue = 100;
        healthSlider.value = 100;

        if (playerNetworkStats != null)
        {
            playerNetworkStats.health.OnValueChanged += OnHealthValueChanged;
        }
    }

    private void OnHealthValueChanged(int previousValue, int newValue)
    {
        healthSlider.value = newValue;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (playerNetworkStats != null)
        {
            playerNetworkStats.health.OnValueChanged -= OnHealthValueChanged; 
        }
    }
}
