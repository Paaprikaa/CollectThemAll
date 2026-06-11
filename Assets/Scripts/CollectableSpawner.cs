using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CollectableSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _collectable;
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private Dictionary<GameObject, bool> _collected = new Dictionary<GameObject, bool>();

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
        if (_collected.Count == 0)
        {
            foreach (Transform transform in _spawnPoints)
            {
                GameObject obj = Instantiate(_collectable, transform.position, Quaternion.identity, gameObject.transform);
                obj.GetComponent<NetworkObject>().Spawn();
                _collected[obj] = false;
            }
        }
        else
        {
            foreach (var key in _collected.Keys)
            {
                key.SetActive(true);
            }
        }
    }

    // calls FinishGame if all objects where collected
    public void UpdateCollectables(NetworkObject netObj, ulong playerClientId)
    {
        netObj.Despawn(false);
        netObj.gameObject.SetActive(false);

        _collected[netObj.gameObject] = true;

        NetworkManager.Singleton.ConnectedClients[playerClientId].PlayerObject.GetComponent<Player>().collected.Value++;

        if (!_collected.ContainsValue(false)) GameManager.Instance.FinishGame();
    }

}