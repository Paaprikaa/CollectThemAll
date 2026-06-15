using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectTrigger : NetworkBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (!IsSpawned) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        Player player = other.GetComponent<Player>();
        if (playerNetObj == null) return;
        if (Keyboard.current.eKey.isPressed && playerNetObj.IsOwner && player.carriedCollectableId < ulong.MaxValue)
        {
            CollectRpc(player.carriedCollectableId, playerNetObj.OwnerClientId);
            player.Collect();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CollectRpc(ulong collectedId, ulong playerClientId)
    {
        CollectableSpawner.Instance.UpdateCollectables(collectedId, playerClientId);
    }

}