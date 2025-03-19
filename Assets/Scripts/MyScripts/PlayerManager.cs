using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public enum PlayerClass { Butler, Ghost };

public class PlayerManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnClassChange))]
    public PlayerClass playerClass;
    public Transform cameraTransform {  get; private set; }
    public MeshRenderer butlerMeshRenderer, ghostMeshRenderer, eyesMeshRenderer;
    public ButlerFlashlight butlerFlashlight;

    [SerializeField]
    int selfLayer = 7;

    PlayerMovement playerMovement;

    [Header("Banishment")]
    public float banishmentTotalTime = 2;
    [SyncVar(hook = nameof(OnBanishmentTimeChange))]
    public float banishmentCurrentTime = 0;
    public float banishmentLastTime = 0;
    public float banishmentMatchTimeReduction = 5;
    bool banishing = false;

    [Header("Possession")]
    public CursableObject possessedObject;

    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        playerMovement = GetComponent<PlayerMovement>();

        if (isLocalPlayer)
        {
            FindFirstObjectByType<UIManager>()?.ChangeClassUI(playerClass);
            butlerMeshRenderer.gameObject.layer = ghostMeshRenderer.gameObject.layer = selfLayer;
        }
    }

    void LateUpdate()
    {
        if (!isServer) return;

        #region Banishment
        if (banishmentCurrentTime > 0) 
        {
            if (banishmentCurrentTime >= banishmentTotalTime)
            {
                ServerBanishPlayer();
                return;
            }
            if (banishmentLastTime == banishmentCurrentTime)
            {
                banishmentCurrentTime = Mathf.Max(banishmentCurrentTime - Time.deltaTime, 0f);
                if (banishing)
                {
                    banishing = false;

                    if (playerClass == PlayerClass.Ghost)
                        playerMovement.ServerFlashlightSlowdown(false);
                }
            }
            banishmentLastTime = banishmentCurrentTime;
        }
        if (banishmentCurrentTime < 0)
        {
            banishmentCurrentTime = Mathf.Min(banishmentCurrentTime + Time.deltaTime, 0f);
        }
        #endregion

        #region Possession

        #endregion
    }

    void OnClassChange(PlayerClass oldClass, PlayerClass newClass)
    {
        switch (newClass)
        {
            case PlayerClass.Butler:
                tag = "Butler";
                butlerMeshRenderer.enabled = true;
                ghostMeshRenderer.enabled = false;
                butlerFlashlight.gameObject.SetActive(true);
                break;
            case PlayerClass.Ghost:
                tag = "Ghost";
                butlerMeshRenderer.enabled = false;
                ghostMeshRenderer.enabled = true;
                butlerFlashlight.gameObject.SetActive(false);
                break;
        }
    }

    void OnBanishmentTimeChange(float oldValue, float newValue)
    {
        if (isLocalPlayer)
        {
            UIManager.singleton.progressBar.fillAmount = newValue / banishmentTotalTime;
            UIManager.singleton.progressBarPanel.SetActive(newValue > 0);
        }
    }

    public void DestroyPlayer()
    {
        CmdDestroyPlayer();
    }

    [Command]
    void CmdDestroyPlayer()
    {
        NetworkServer.Destroy(gameObject);
    }

    public void SecondaryInput(InputAction.CallbackContext inputContext)
    {
        if (inputContext.started)
        {
            switch (playerClass)
            {
                case PlayerClass.Butler:
                    CmdToggleFlashlight();
                    break;
                case PlayerClass.Ghost:
                    possessedObject?.PlayCursedAnimation();
                    break;
            }
        }
    }

    [Command]
    void CmdToggleFlashlight()
    {
        RpcToggleFlashlight(!butlerFlashlight.lightOn);
    }

    [ClientRpc]
    void RpcToggleFlashlight(bool value)
    {
        butlerFlashlight.ToggleFlashlight(value);
    }

    [Server]
    public void ServerFlashlightBanishment(PlayerManager owner)
    {
        owner.banishmentCurrentTime = banishmentCurrentTime += Time.deltaTime;

        if (!banishing && banishmentCurrentTime >= 0)
        {
            owner.banishing = banishing = true;

            if (playerClass == PlayerClass.Ghost)
                playerMovement.ServerFlashlightSlowdown(true);
        }
    }

    [Server]
    void ServerBanishPlayer()
    {
        banishmentLastTime = 0;
        banishmentCurrentTime = -3 * banishmentTotalTime;
        banishing = false;

        if (playerClass == PlayerClass.Ghost)
        {
            playerMovement.ServerFlashlightSlowdown(false);
            playerMovement.respawning = true;
            TargetRespawn(NetManager.singleton.GetSpawnPoint(playerClass).position);
            MatchManager.singleton.ServerReduceMatchTime(banishmentMatchTimeReduction);
        }
    }

    [TargetRpc]
    void TargetRespawn(Vector3 pos)
    {
        transform.position = pos;
        playerMovement.CmdRespawn();
    }

    [Command]
    public void CmdReduceMatchTime(float time)
    {
        MatchManager.singleton.ServerReduceMatchTime(time);
    }

    public void StartPossession(CursableObject cursableObject)
    {
        possessedObject = cursableObject == possessedObject ? null : cursableObject;
        playerMovement.characterController.enabled = ghostMeshRenderer.enabled = eyesMeshRenderer.enabled = !possessedObject;
        transform.position = possessedObject ? possessedObject.transform.position + Vector3.up * -1 : transform.position;
    }

    public void CompletePossession(CursableObject cursableObject)
    {
        possessedObject = cursableObject == possessedObject ? null : cursableObject;
        playerMovement.characterController.enabled = ghostMeshRenderer.enabled = eyesMeshRenderer.enabled = !possessedObject;
        transform.position = possessedObject ? possessedObject.transform.position + Vector3.up * -1 : transform.position;
    }

    public void UnpossessObject(InputAction.CallbackContext inputContext)
    {
        if (inputContext.started)
        {
            if (possessedObject)
                StartPossession(null);
        }
    }
}