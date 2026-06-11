using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public string playerName;
    public NetworkVariable<int> collected = new NetworkVariable<int>();

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        collected.Value = 0;
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        _meshRenderer.material.SetColor("_BaseColor", PlayerColors.playerListColors[(int)OwnerClientId]);
    }
}
