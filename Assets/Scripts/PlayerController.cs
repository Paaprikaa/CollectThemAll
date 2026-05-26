using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    private CharacterController _characterController;
    private Vector2 _input;
    private float _yVelocity;
    private float _gravity = -7f;



    [Header("Camera")]
    [SerializeField] GameObject _playerCamera;
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
    public override void OnNetworkSpawn()
    {
        _playerCamera.SetActive(IsOwner);
        GetComponent<PlayerInput>().enabled = IsOwner;
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector3 moveDirection = GetCameraRelativeDirection();

        RotateCharacter();
        moveDirection = ApplyGravity(moveDirection);
        MoveCharacter(moveDirection);
    }

    private Vector3 GetCameraRelativeDirection()
    {
        Vector3 camForward = _playerCamera.transform.forward;
        Vector3 camRight = _playerCamera.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        return camRight * _input.x + camForward * _input.y;
    }

    private Vector3 ApplyGravity(Vector3 moveDirection)
    {
        if (_characterController.isGrounded && _yVelocity < 0)
        {
            _yVelocity = -2f;
        }

        _yVelocity += _gravity * Time.deltaTime;
        moveDirection.y = _yVelocity;

        return moveDirection;
    }

    private void RotateCharacter()
    {
        Vector2 rotationInput = new Vector2(_input.x, Mathf.Max(_input.y, 0f));

        if (rotationInput.magnitude <= 0.1f) return;

        Vector3 camForward = _playerCamera.transform.forward;
        Vector3 camRight = _playerCamera.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 rotationDirection = camRight * rotationInput.x + camForward * rotationInput.y;

        if (rotationDirection.magnitude <= 0.1f) return;

        Quaternion targetRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 340f * Time.deltaTime);
    }

    private void MoveCharacter(Vector3 moveDirection)
    {
        _characterController.Move(moveDirection * _speed * Time.deltaTime);
    }

    public void OnMove(InputValue value)
    {
        if (!IsOwner) return;

        _input = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!IsOwner) return;
        if (!_characterController.isGrounded) return;

        _yVelocity = _jumpForce;
    }
}