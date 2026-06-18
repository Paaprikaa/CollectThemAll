using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CollectableSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _collectable;
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private Dictionary<ulong, CollectableData> _collectables = new Dictionary<ulong, CollectableData>();

    public static CollectableSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public void SetCollectables()
    {
        if (_collectables.Count == 0)
        {
            foreach (Transform transform in _spawnPoints)
            {
                GameObject obj = Instantiate(_collectable, transform.position, Quaternion.identity, gameObject.transform);
                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();
                _collectables[netObj.NetworkObjectId] = new CollectableData { GameObject = obj, IsCollected = false };
            }
        }
        else
        {
            // rebuild dictionary with new NetworkObjectIds
            List<CollectableData> existingData = _collectables.Values.ToList();
            _collectables.Clear();

            foreach (var data in existingData)
            {
                data.IsCollected = false;
                data.GameObject.SetActive(true);
                NetworkObject netObj = data.GameObject.GetComponent<NetworkObject>();
                netObj.Spawn();
                _collectables[netObj.NetworkObjectId] = data;
            }
        }
    }

    public void UpdateCollectablesCarry(NetworkObject netObj, ulong playerClientId)
    {
        // player carries
        netObj.Despawn(false);
        netObj.gameObject.SetActive(false);
    }

    public void UpdateCollectables(ulong collectedId, ulong playerClientId)
    {
        // player collects
        _collectables[collectedId].IsCollected = true;

        Player player = NetworkManager.Singleton.ConnectedClients[playerClientId].PlayerObject.GetComponent<Player>();
        player.collected.Value++;
        GameManager.Instance.UpdateCollectablesUiRpc((int)playerClientId, player.collected.Value);

        if (_collectables.Values.All(data => data.IsCollected)) GameManager.Instance.FinishGame();
    }

}