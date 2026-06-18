using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    private Transform _camTransform;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        _playerCamera.SetActive(IsOwner);
        GetComponent<PlayerInput>().enabled = IsOwner;

        if (!IsOwner) return;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
        }
        else
        {
            _camTransform = Camera.main.transform;
        }
    }

    public void LockCursor()
    {
        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnLockCursor()
    {
        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        _camTransform = Camera.main.transform;

        GameManager.Instance.OnMatchStarted += LockCursor;
        GameManager.Instance.OnMatchFinished += UnLockCursor;

        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (_camTransform == null) return;

        Vector3 moveDirection = GetCameraRelativeDirection();

        RotateCharacter(moveDirection);
        moveDirection = ApplyGravity(moveDirection);
        MoveCharacter(moveDirection);
    }

    private Vector3 GetCameraRelativeDirection()
    {
        Vector3 camForward = _camTransform.transform.forward;
        Vector3 camRight = _camTransform.transform.right;

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

    private void RotateCharacter(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
    }

    private void MoveCharacter(Vector3 moveDirection)
    {
        if (!_characterController.enabled) return;
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