using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CollectableSpawner.Instance.SetCollectables();
    }

    public void FinishGame()
    {
        Debug.Log("game finished"); // TODO
    }
}
