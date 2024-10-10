using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    PlayerManager playerManager;

    CharacterController characterController;
    Vector2 movementInput;
    Vector3 moveDirection;
    Vector2 lookInput;
    float verticalLookRot;

    [SerializeField]
    float movementSpeed = 5;
    [SerializeField]
    float jumpPower = 5;
    [SerializeField]
    float gravity = 10;
    [SerializeField]
    float lookSpeed = 1;
    [SerializeField]
    float verticalLookLimit = 60;

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        characterController = GetComponent<CharacterController>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        GetComponent<PlayerInput>().enabled = true;
        GetComponentInChildren<Camera>().enabled = true;
        GetComponentInChildren<AudioListener>().enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        #region Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float moveDirectionY = moveDirection.y;
        moveDirection = (forward * movementInput.y * movementSpeed) + (right * movementInput.x * movementSpeed);
        #endregion

        #region Gravity
        moveDirection.y = moveDirectionY;
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;
        #endregion

        characterController.Move(moveDirection * Time.deltaTime);

        #region Look and Rotation
        verticalLookRot += lookInput.y * lookSpeed;
        verticalLookRot = Mathf.Clamp(verticalLookRot, -verticalLookLimit, verticalLookLimit);
        if(playerManager.cameraTransform)
            playerManager.cameraTransform.localRotation = Quaternion.Euler(-verticalLookRot, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
        #endregion
    }

    public void PlayerMove(InputAction.CallbackContext inputContext)
    {
        movementInput = inputContext.ReadValue<Vector2>();
    }

    public void PlayerJump(InputAction.CallbackContext inputContext)
    {
        if (characterController.isGrounded)
            moveDirection.y = jumpPower;
    }

    public void PlayerLook(InputAction.CallbackContext inputContext)
    {
        lookInput = inputContext.ReadValue<Vector2>();
    }
}