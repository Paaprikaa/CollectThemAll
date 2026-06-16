using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarryTrigger : NetworkBehaviour
{
    [SerializeField] private GameObject _pressE;

    private void OnTriggerEnter(Collider other)
    {
        _pressE.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        _pressE.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsSpawned) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        Player player = other.GetComponent<Player>();
        if (playerNetObj == null) return;
        if (Keyboard.current.eKey.isPressed && playerNetObj.IsOwner && player.carriedCollectableId == ulong.MaxValue)
        {
            player.Carry(NetworkObjectId);
            CarryRpc(NetworkObjectId, playerNetObj.OwnerClientId);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CarryRpc(ulong collectedId, ulong playerClientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(collectedId, out NetworkObject netObj))
        {
            CollectableSpawner.Instance.UpdateCollectablesCarry(netObj, playerClientId);
        }
    }
}
