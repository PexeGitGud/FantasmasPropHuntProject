using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using Mirror.Examples.Benchmark;

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
    public float possessionTotalTime = 2;
    [SyncVar(hook = nameof(OnPossessionTimeChange))]
    public float possessionCurrentTime = 0;
    CursableObject tryingToPossessObject;
    bool possessioning = false;

    [Header("Cursing")]
    public float cursingTotalTime = 2;
    [SyncVar(hook = nameof(OnCursingTimeChange))]
    public float cursingCurrentTime = 0;
    bool cursing = false;

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
            }
            else
            {
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
        }
        if (banishmentCurrentTime < 0)
        {
            banishmentCurrentTime = Mathf.Min(banishmentCurrentTime + Time.deltaTime, 0f);
        }
        #endregion

        #region Possession
        if (possessioning)
        {
            possessionCurrentTime += Time.deltaTime;

            if (possessionCurrentTime >= possessionTotalTime)
            {
                ServerCompletePossession(tryingToPossessObject);
                ServerStopPossession();
            }
        }
        #endregion

        #region Cursing
        if (cursing)
        {
            cursingCurrentTime += Time.deltaTime;

            if (cursingCurrentTime >= cursingTotalTime)
            {
                ServerCompleteCursing();
                ServerStopCursing();
            }
        }
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
            UIManager.singleton.UpdateProgressBar(newValue / banishmentTotalTime, (ProgressBarType)playerClass);
    }

    void OnPossessionTimeChange(float oldValue, float newValue)
    {
        if (isLocalPlayer)
            UIManager.singleton.UpdateProgressBar(newValue / possessionTotalTime, ProgressBarType.Possession);
    }

    void OnCursingTimeChange(float oldValue, float newValue)
    {
        if (isLocalPlayer)
            UIManager.singleton.UpdateProgressBar(newValue / cursingTotalTime, ProgressBarType.Cursing);
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

    [Server]
    public void ServerInteractionNull()
    {
        switch (playerClass)
        {
            case PlayerClass.Butler:
                break;
            case PlayerClass.Ghost:
                if (possessedObject || possessioning)
                    ServerStartPossession(null);
                break;
        }
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
                    CmdStartCursing();
                    //possessedObject?.PlayCursedAnimation();
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
            {
                playerMovement.ServerFlashlightSlowdown(true);

                if (possessioning)
                    ServerStopPossession();
            }
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

    [Server]
    public void ServerStartPossession(CursableObject cursableObject)
    {
        if (banishing || cursing) return;

        if (possessioning)
        {
            ServerStopPossession();
            return;
        }

        possessioning = true;
        tryingToPossessObject = possessedObject == null ? cursableObject : null;
        RpcStartStopPossession(true);
    }

    [Server]
    public void ServerStopPossession()
    {
        possessioning = false;
        tryingToPossessObject = null;
        possessionCurrentTime = 0;
        RpcStartStopPossession(possessedObject == null);
    }

    [Server]
    public void ServerCompletePossession(CursableObject cursableObject) 
    {
        RpcCompletePossession(cursableObject);
    }

    [ClientRpc]
    void RpcCompletePossession(CursableObject cursableObject)
    {
        possessedObject = cursableObject;
        playerMovement.characterController.enabled = ghostMeshRenderer.enabled = eyesMeshRenderer.enabled = possessedObject == null;
        transform.position = possessedObject == null ? transform.position : possessedObject.transform.position + Vector3.up * -1;
    }

    [ClientRpc]
    void RpcStartStopPossession(bool start)
    {
        playerMovement.characterController.enabled = !start;
    }

    [Command]
    void CmdStartCursing()
    {
        if (possessioning || !possessedObject) return;

        if (cursing)
        {
            ServerStopCursing();
            return;
        }

        cursing = true;
    }

    [Server]
    void ServerStopCursing()
    {
        cursing = false;
        cursingCurrentTime = 0;
    }

    [Server]
    void ServerCompleteCursing()
    {
        ServerCompletePossession(null);
        RpcCompleteCursing(possessedObject);
    }

    [ClientRpc]
    void RpcCompleteCursing(CursableObject possessedObject)
    {
        possessedObject.PlayCursedAnimation();
    }
}