using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public string playerName;
    public int collected;

    [SerializeField] private MeshRenderer _meshRenderer;

    private void Awake()
    {
        collected = 0;
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        _meshRenderer.material.SetColor("_BaseColor", PlayerColors.playerListColors[(int)OwnerClientId]);
    }
}
