using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action OnMatchStarted;
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
    [SerializeField] private TextMeshProUGUI _endGameResultText;

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
            int points = (int)connectedClient.PlayerObject.GetComponent<Player>().collected.Value;
            playerPoints[connectedClient.ClientId] = points;
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

        FinishGameRpc(resultText);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    public void FinishGameRpc(string resultText)
    {
        matchStarted = false;
        _endGamePanel.SetActive(true);
        _endGameResultText.text = resultText;
    }

    public void GoMainMenu() { Debug.Log("mainmenuuuu"); }

    public void PlayAgain() { Debug.Log("playagainnn"); }
}
