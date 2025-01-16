using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports;
using System.Collections.Generic;


public class UserI : MonoBehaviour
{
    private string address = "127.0.0.1";

    [SerializeField] private NetworkTransport transport;
    private bool isReady = false;
    [SerializeField] private GameObject uiGameObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    private void OnGUI()
    {
        if (NetworkManager.Singleton == null)
            return;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Normal,
            padding = new RectOffset(8, 8, 5, 5),
            normal = new GUIStyleState
            {
                background = MakeTexture(2, 2, new Color(0.5f, 0.4f, 0.9f, 0.5f))
            },
            hover = new GUIStyleState
            {
                background = MakeTexture(2, 2, new Color(0.4f, 0.2f, 0.7f, 0.5f))
            }
        };

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.3f, 1.0f, 0.5f, 0.8f) }
        };
        GUIStyle labelStyle2 = new GUIStyle(GUI.skin.label)
        {
            fontSize = 8,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter
        };

        float buttonHeight = 20;
        float buttonSpacing = 5;
        float labelHeight = 20;
        float totalHeight = labelHeight + (buttonHeight + buttonSpacing) * 3;
        float width = 160;

        float x = 50;
        float y = 70;

        GUI.Box(new Rect(x, y, width, totalHeight), GUIContent.none);

        GUILayout.BeginArea(new Rect(x, y, width, totalHeight));

        GUILayout.Label("\"Q\" to toggle cursor", labelStyle2);
        GUILayout.Space(buttonSpacing);
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            if (GUILayout.Button("Host", buttonStyle, GUILayout.Height(buttonHeight)))
            {
                if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
                {
                    NetworkManager.Singleton.StartHost();
                }
            }

            if (GUILayout.Button("Join", buttonStyle, GUILayout.Height(buttonHeight)))
            {
                if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
                {
                    NetworkManager.Singleton.StartClient();
                }
            }

            address = GUILayout.TextField(address, GUILayout.Height(buttonHeight));
            UpdateTransport();
        }
        else
        {
            if (GUILayout.Button(isReady ? "Cancel Ready" : "Ready", buttonStyle, GUILayout.Height(buttonHeight)))
            {
                isReady = !isReady;
                CheckReady();
            }

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            {
                if (GUILayout.Button("Disconnect", buttonStyle, GUILayout.Height(buttonHeight)))
                {
                    NetworkManager.Singleton.Shutdown();
                }
            }
        }

        GUILayout.EndArea();
    }

    private void CheckReady()
    {
        if (isReady)
        {
            StartGame();
        }
        else
        {
            if (uiGameObject != null)
            {
                uiGameObject.SetActive(true); 
            }
            var foundInputs = FindObjectsByType<InputActionsScript>(FindObjectsSortMode.None);
            foreach (var input in foundInputs) 
            {
                input.GetComponent<InputActionsScript>().changeChat(true);
            }
        }
    }

    private void StartGame()
    {
        if (uiGameObject != null)
        {
            uiGameObject.SetActive(false);
        }
        var foundInputs = FindObjectsByType<InputActionsScript>(FindObjectsSortMode.None);
        foreach (var input in foundInputs)
        {
            input.GetComponent<InputActionsScript>().changeChat(false);
        }
    }

    private void UpdateTransport()
    {
        if (transport is UnityTransport unityTransport)
        {
            unityTransport.SetConnectionData(address, 7777);
        }
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}