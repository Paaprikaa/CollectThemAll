using UnityEngine;

public class CollectableAnimation : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;

    [Header("Oscillation")]
    [SerializeField] private float _oscillationHeight = 0.3f;
    [SerializeField] private float _oscillationSpeed = 1f;

    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime);

        float newY = _startPosition.y + Mathf.Sin(Time.time * _oscillationSpeed) * _oscillationHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}