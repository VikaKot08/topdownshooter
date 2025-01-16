using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
[RequireComponent(typeof(PlayerInput))]
#endif
public class ThirdPersonController : MonoBehaviour
{
    public InputActionsScript input;
    public PlayerStatsUI gameUIPrefab;

    private void Start()
    {
        input = GetComponent<InputActionsScript>();

        PlayerStatsUI statsUI = Instantiate(gameUIPrefab);
        PlayerNetworkStats playerStats = GetComponent<PlayerNetworkStats>();
        statsUI.Initialize(playerStats);
        statsUI.enabled = true;


    }
}