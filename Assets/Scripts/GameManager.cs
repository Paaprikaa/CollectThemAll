using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action OnMatchStarted;
    public event Action OnMatchFinished;
    public float timeRemaining = 300f;
    public List<Transform> playerSpawnPoints = new List<Transform>();

    [Header("InGame UI")]
    [SerializeField] private List<GameObject> _playerPanels = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI _timer;

    [Header("Pre-Match UI")]
    [SerializeField] private GameObject _buttonStartMatch;
    [SerializeField] private GameObject _textNeedClients;
    [SerializeField] private GameObject _textWaitHost;
    [SerializeField] private GameObject _initialWalls;

    [Header("End UI")]
    [SerializeField] private GameObject _endGamePanel;
    [SerializeField] private GameObject _endGameButtons;
    [SerializeField] private TextMeshProUGUI _endGameResultText;
    [SerializeField] private GameObject _endGameConfirmedText;

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

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
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
    public void PlayerEnterRpc(int playerId, string playerName)
    {
        _playerPanels[playerId].SetActive(true);

        SessionData.Instance.PlayerNames[(ulong)playerId] = playerName;

        TextMeshProUGUI textCollectables = _playerPanels[playerId].GetComponentInChildren<TextMeshProUGUI>();
        if (textCollectables != null) textCollectables.text = playerName + ": " + 0;
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
        if (!IsServer) return;

        Dictionary<ulong, int> playerPoints = new();
        foreach (var connectedClient in NetworkManager.Singleton.ConnectedClients.Values)
        {
            Player player = connectedClient.PlayerObject.GetComponent<Player>();
            int points = player.collected.Value;
            playerPoints[connectedClient.ClientId] = points;

            player.isReady.Value = false;
        }

        int maxValue = playerPoints.Values.Max();
        var topPlayers = playerPoints.Where(kvp => kvp.Value == maxValue).Select(kvp => kvp.Key).ToList();

        string resultText;
        if (topPlayers.Count == 1)
        {
            resultText = SessionData.Instance.PlayerNames[topPlayers[0]] + " wins with " + maxValue + " points!";
        }
        else
        {
            resultText = "Draw, max points: " + maxValue + "\nWant a rematch?";
        }

        OnMatchFinished?.Invoke();

        FinishGameRpc(resultText);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    public void FinishGameRpc(string resultText)
    {
        matchStarted = false;
        _endGamePanel.SetActive(true);
        _endGameResultText.text = resultText;
    }

    public void GoMainMenu()
    {
        if (IsServer)
        {
            GoMainMenuRpc();
        }
        else
        {
            LeaveGame();
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    private void GoMainMenuRpc()
    {
        LeaveGame(); // disconnect all
    }

    private void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayAgain()
    {
        _endGameButtons.SetActive(false);
        _endGameConfirmedText.SetActive(true);

        RequestPlayAgainRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestPlayAgainRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>().isReady.Value = true;

        bool allReady = true;
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            allReady = allReady && client.PlayerObject.GetComponent<Player>().isReady.Value;
        }

        if (allReady) ResetGameRpc();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    private void ResetGameRpc()
    {
        // restart endgame ui
        _endGameButtons.SetActive(true);
        _endGameConfirmedText.SetActive(false);
        _endGamePanel.SetActive(false);

        // restart waiting room UI
        if (IsHost) _buttonStartMatch.SetActive(true);
        if (IsClient && !IsHost) _textWaitHost.SetActive(true);

        if (!IsServer) return;

        // restart collectables
        CollectableSpawner.Instance.SetCollectables();

        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            Player player = client.PlayerObject.GetComponent<Player>();
            player.isReady.Value = false;
            // restart points
            player.collected.Value = 0;
            // restart match UI
            UpdateCollectablesUiRpc((int)client.ClientId, player.collected.Value);

            //TODO: implement this
            // reastart positions
            //Vector3 spawnPos = playerSpawnPoints[(int)client.ClientId].position;
            //Quaternion spawnRot = playerSpawnPoints[(int)client.ClientId].rotation;
            //player.SetSpawnPointRpc(spawnPos, spawnRot,false);
        }

        //TODO: activate when spawn points solved
        //_initialWalls.SetActive(true);

        // restart timer
        timeRemaining = 300f;
    }


    private void OnClientDisconnected(ulong clientId)
    {
        _playerPanels[(int)clientId].SetActive(false);
        SessionData.Instance.PlayerNames.Remove(clientId);
    }
}
