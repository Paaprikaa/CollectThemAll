using UnityEngine;

public class CollectableAnimation : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;

    [Header("Oscillation")]
    [SerializeField] private float _oscillationHeight = 0.3f;
    [SerializeField] private float _oscillationSpeed = 1f;


    private void Update()
    {
        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime);

        float oscillation = Mathf.Sin(Time.time * _oscillationSpeed) * _oscillationHeight;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y + oscillation * Time.deltaTime,
            transform.localPosition.z
        );
    }
}