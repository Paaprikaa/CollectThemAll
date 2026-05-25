using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _connecting;
    [SerializeField] private GameObject _hostPanel;
    [SerializeField] private GameObject _clientPanel;
    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject _goBackButton;
    [SerializeField] private TMP_InputField _ipInputField;
    [SerializeField] private TextMeshProUGUI _yourIpText;
    [SerializeField] private TextMeshProUGUI _wrongIpText;
    [SerializeField] private TextMeshProUGUI _connectionErrorText;
    [SerializeField] private TextMeshProUGUI _attempconetText;

    public int maxPlayers = 2;

    private UnityTransport transport;
    private string _yourIp;
    private List<bool> _buttonSelected = new List<bool> { false, false }; // 0: host, 1: client

    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        _yourIp = GetIP();

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    // BUTTONS

    public void Host()
    {
        if (_buttonSelected[0])
        {
            _panel.SetActive(false);
            _buttonSelected[0] = false;
            _buttonSelected[1] = false;
        }
        else
        {
            _panel.SetActive(true);
            _buttonSelected[0] = true;
            _buttonSelected[1] = false;

            _yourIpText.text = $"Your IP is: {_yourIp}\r\nShare it with your friends to join in!";
        }
        _hostPanel.SetActive(_buttonSelected[0]);
        _clientPanel.SetActive(_buttonSelected[1]);
    }

    public void Join()
    {
        if (_buttonSelected[1])
        {
            _panel.SetActive(false);
            _buttonSelected[0] = false;
            _buttonSelected[1] = false;
        }
        else
        {
            _panel.SetActive(true);
            _buttonSelected[0] = false;
            _buttonSelected[1] = true;
        }
        _hostPanel.SetActive(_buttonSelected[0]);
        _clientPanel.SetActive(_buttonSelected[1]);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void CreateRoom()
    {
        transport.SetConnectionData("0.0.0.0", 7777);
        NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    public void JoinRoom()
    {
        string ip = _ipInputField.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            _wrongIpText.text = "You need to enter an IP address.";
            return;
        }
        if (!System.Net.IPAddress.TryParse(ip, out _))
        {
            _wrongIpText.text = "Invalid IP format.";
            return;
        }

        transport.SetConnectionData(ip, 7777);


        NetworkManager.Singleton.StartClient();
        _mainMenu.SetActive(false);
        _connecting.SetActive(true);
        _attempconetText.text = $"Attempting to connect to: {ip}";
    }

    public void GoBack()
    {
        _mainMenu.SetActive(true);
        _hostPanel.SetActive(false);
        _clientPanel.SetActive(false);
        _panel.SetActive(false);
        _connecting.SetActive(false);
        _goBackButton.SetActive(false);
        _ipInputField.text = "";
        _wrongIpText.text = "";
        _connectionErrorText.text = "Please wait :)";
        _attempconetText.text = "";
    }

    // NETWORKING

    private string GetIP()
    {
        string ip = "";
        foreach (var dir in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))
        {
            if (dir.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ip = dir.ToString();
                return ip;
            }
        }
        return "unavailable";
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (NetworkManager.Singleton.DisconnectReason != "")
            {
                _connectionErrorText.text = $"An error has occurred: {NetworkManager.Singleton.DisconnectReason}";
            }
            else
            {
                _connectionErrorText.text = $"An error has occurred";
            }
            _goBackButton.SetActive(true);
        }
    }

    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        int currentPlayers = NetworkManager.Singleton.ConnectedClients.Count;

        if (currentPlayers < maxPlayers )
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
        else
        {
            response.Approved = false;
            response.Reason = $"Room is full (max {maxPlayers} players).";
        }
    }

}