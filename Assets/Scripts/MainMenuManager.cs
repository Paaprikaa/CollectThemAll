using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject _hostPanel;
    [SerializeField] private GameObject _clientPanel;
    [SerializeField] private GameObject _panel;
    private UnityTransport transport;
    private string _IP;
    private List<bool> _buttonSelected = new List<bool> { false, false }; // 0: host, 1: client

    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        _IP = GetIP();
        Debug.Log($"IP: {_IP}");
    }

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


    private string GetIP()
    {
        string ip = "";
        foreach (var dir in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))
        {
            if (dir.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ip = dir.ToString();
                break;
            }
        }
        return ip;
    }
}