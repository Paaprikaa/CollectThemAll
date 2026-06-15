using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    public string playerName;
    public NetworkVariable<int> collected = new NetworkVariable<int>();

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        collected.Value = 0;
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        _meshRenderer.material.SetColor("_BaseColor", PlayerColors.playerListColors[(int)OwnerClientId]);
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
    }

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!IsServer) return;

        GameManager.Instance.PlayerEnterRpc((int)clientId);

        foreach (var connectedClient in NetworkManager.Singleton.ConnectedClients.Values)
        {
            int connectedPlayerId = (int)connectedClient.PlayerObject.GetComponent<Player>().OwnerClientId;

            if (connectedPlayerId != (int)OwnerClientId) GameManager.Instance.PlayerEnterRpc(connectedPlayerId);
        }

        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
    }
}
