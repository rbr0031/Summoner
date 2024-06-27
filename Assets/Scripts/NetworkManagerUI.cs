using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    [SerializeField] private List<GameObject> playerPrefabs;

    private void Awake()
    {
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

         NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Assign a unique prefab to the new client
            int prefabIndex = (int)(clientId % (ulong)playerPrefabs.Count);
            GameObject playerPrefab = playerPrefabs[prefabIndex];

            // Instantiate and spawn the player object
            GameObject playerInstance = Instantiate(playerPrefab);
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);
        }
    }
}
