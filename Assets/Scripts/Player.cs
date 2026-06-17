using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    public string playerName;
    public NetworkVariable<int> collected = new NetworkVariable<int>();
    public ulong carriedCollectableId { get; private set; }
    [SerializeField] private GameObject _collectablePrefabCarry;
    [SerializeField] private List<Material> _playerColors;

    private void Awake()
    {
        collected.Value = 0;
        carriedCollectableId = ulong.MaxValue;
        _collectablePrefabCarry.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = _playerColors[(int)OwnerClientId];
        }

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;

        playerName = SessionData.Instance.PlayerNames[OwnerClientId];
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

    public void Carry(ulong collectedId)
    {
        _collectablePrefabCarry.SetActive(true);
        carriedCollectableId = collectedId;
    }

    public void Collect()
    {
        _collectablePrefabCarry.SetActive(false);
        carriedCollectableId = ulong.MaxValue;
    }
}
