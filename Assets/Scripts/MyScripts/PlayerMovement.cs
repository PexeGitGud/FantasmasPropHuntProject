using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using DG.Tweening;

public class PlayerMovement : NetworkBehaviour
{
    PlayerManager playerManager;

    CharacterController characterController;
    Vector2 movementInput;
    Vector3 moveDirection;
    Vector2 lookInput;
    float verticalLookRot;

    [SyncVar(hook = nameof(FlashlightSlowdownChange))]
    bool flashlightSlowdown = false;
    [SyncVar]
    public bool respawning = false;

    [SerializeField]
    float movementSpeed = 5;
    [SerializeField]
    float slowdpwnModifier = .5f;
    [SerializeField]
    float jumpPower = 5;
    [SerializeField]
    float gravity = 10;
    [SerializeField]
    float lookSpeed = 1;
    [SerializeField]
    float verticalLookLimit = 60;

    Tween slowShakeTween;// => transform.GetChild(0).DOShakeRotation(.5f, 5, 10, 90, true, ShakeRandomnessMode.Harmonic).SetRelative().SetEase(Ease.InOutElastic);

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        characterController = GetComponent<CharacterController>();

        #region DOTween
        DOTween.Init();
        slowShakeTween = transform.GetChild(0).DOShakeRotation(.5f, 5, 10, 90, true, ShakeRandomnessMode.Harmonic).SetRelative().SetEase(Ease.InOutElastic);
        slowShakeTween.onComplete = () => slowShakeTween.Rewind();
        slowShakeTween.onRewind = () => { if (flashlightSlowdown) slowShakeTween.Play(); Debug.Log("Rewind"); };
        #endregion
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
        if (respawning) return;

        //if (flashlightSlowdown)
        //{
        //    slowShakeTween.Play();
        //}

        if (!isLocalPlayer) return;

        #region Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float moveDirectionY = moveDirection.y;
        moveDirection = (forward * movementInput.y + right * movementInput.x) * movementSpeed * (flashlightSlowdown ? slowdpwnModifier : 1);
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
        if (playerManager.cameraTransform)
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

    void FlashlightSlowdownChange(bool oldFlashlightSlowdown, bool newFlashlightSlowdown)
    {
        if (newFlashlightSlowdown)
            slowShakeTween.Play();
        Debug.Log("ClientPlay");
    }

    [Server]
    public void ServerFlashlightSlowdown(bool slowing)
    {
        flashlightSlowdown = slowing;
    }

    [Command]
    public void CmdRespawn()
    {
        respawning = false;
    }
}