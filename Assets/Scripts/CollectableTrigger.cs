using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectableTrigger : NetworkBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (!IsSpawned) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null) return;
        if (Keyboard.current.eKey.isPressed && playerNetObj.IsOwner)
        {
            CollectRpc(NetworkObjectId , playerNetObj.OwnerClientId);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CollectRpc(ulong collectedId, ulong playerClientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(collectedId, out NetworkObject netObj))
            CollectableSpawner.Instance.UpdateCollectables(netObj, playerClientId);
    }
}
