using System.Collections.Generic;
using UnityEngine;

public class SessionData : MonoBehaviour
{
    public static SessionData Instance { get; private set; }
    public Dictionary<ulong, string> PlayerNames = new Dictionary<ulong, string>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}