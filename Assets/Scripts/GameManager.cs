using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action OnMatchStarted;
    public float timeRemaining = 300f;

    [SerializeField] private List<GameObject> _playerPanels = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI _timer;

    [SerializeField] private GameObject _buttonStartMatch;
    [SerializeField] private GameObject _textNeedClients;
    [SerializeField] private GameObject _textWaitHost;
    
    [SerializeField] private GameObject _initialWalls;

    public bool matchStarted { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        matchStarted = false;
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            CollectableSpawner.Instance.SetCollectables();
        }

        if (IsHost) _buttonStartMatch.SetActive(true);
        if (IsClient && !IsHost) _textWaitHost.SetActive(true);
    }

    private void Update()
    {
        if (!matchStarted || !IsServer) return;
     
        timeRemaining -= Time.deltaTime;
        UpdateTimerRpc(timeRemaining);

        if (timeRemaining <= 0)
        {
            matchStarted = false;
            FinishGame();
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    public void PlayerEnterRpc(int playerId)
    {
        _playerPanels[playerId].SetActive(true);

        TextMeshProUGUI textCollectables = _playerPanels[playerId].GetComponentInChildren<TextMeshProUGUI>();
        if (textCollectables != null) textCollectables.text = SessionData.Instance.PlayerNames[(ulong)playerId] + ": " + 0;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    public void StartMatchRpc()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count <= 1)
        {
            _textNeedClients.SetActive(true);
            return;
        }

        _textNeedClients.SetActive(false);
        _buttonStartMatch.SetActive(false);
        _textWaitHost.SetActive(false);
        _initialWalls.SetActive(false);

        matchStarted = true;

        OnMatchStarted?.Invoke();
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateTimerRpc(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        _timer.text = $"{minutes}:{seconds:00}";
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateCollectablesUiRpc(int playerId, int collected)
    {
        TextMeshProUGUI textCollectables = _playerPanels[playerId].GetComponentInChildren<TextMeshProUGUI>();
        if (textCollectables != null) textCollectables.text = SessionData.Instance.PlayerNames[(ulong)playerId] + ": " + collected;
    }

    public void FinishGame()
    {
        Debug.Log("game finished"); // TODO
    }
}
