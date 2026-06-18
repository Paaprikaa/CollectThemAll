using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    public NetworkVariable<int> collected = new();
    public NetworkVariable<bool> isReady = new();
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
    }

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!IsServer) return;

        int spawnIndex = (int)OwnerClientId;
        Vector3 spawnPos = GameManager.Instance.playerSpawnPoints[spawnIndex].position;
        Quaternion spawnRot = GameManager.Instance.playerSpawnPoints[spawnIndex].rotation;
        SetSpawnPointRpc(spawnPos, spawnRot);

        GameManager.Instance.PlayerEnterRpc((int)clientId, SessionData.Instance.PlayerNames[clientId]);

        foreach (var connectedClient in NetworkManager.Singleton.ConnectedClients.Values)
        {
            int connectedPlayerId = (int)connectedClient.PlayerObject.GetComponent<Player>().OwnerClientId;

            if (connectedPlayerId != (int)OwnerClientId) GameManager.Instance.PlayerEnterRpc(connectedPlayerId, SessionData.Instance.PlayerNames[(ulong)connectedPlayerId]);
        }

        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
    }

    [Rpc(SendTo.Owner)]
    public void SetSpawnPointRpc(Vector3 position, Quaternion rotation)
    {
        StartCoroutine(ApplySpawnPoint(position, rotation));
    }

    private IEnumerator ApplySpawnPoint(Vector3 position, Quaternion rotation)
    {
        yield return null; // wait one frame for Netcode to finish syncing
        transform.position = position;
        transform.rotation = rotation;
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
