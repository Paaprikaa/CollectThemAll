using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<GameObject> _playerPanels = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI _timer;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            CollectableSpawner.Instance.SetCollectables();
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    public void PlayerEnterRpc(int playerId)
    {
        _playerPanels[playerId].SetActive(true);
    }

    public void FinishGame()
    {
        Debug.Log("game finished"); // TODO
    }
}
