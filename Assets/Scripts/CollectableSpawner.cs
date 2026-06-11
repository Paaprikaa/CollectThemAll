using System.Collections.Generic;
using UnityEngine;

public class CollectableSpawner : MonoBehaviour
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
    public void Collected(GameObject obj)
    {
        _collected[obj] = true;
        if (!_collected.ContainsValue(false)) GameManager.Instance.FinishGame();
    }

}