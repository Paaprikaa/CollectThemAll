using UnityEngine;

public class CollectableTrigger : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("entrer trigger");
        CollectableSpawner.Instance.Collected(gameObject);
    }
}
