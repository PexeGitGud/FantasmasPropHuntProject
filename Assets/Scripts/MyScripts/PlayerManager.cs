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

    [Header("Respawn")]
    public float respawnTotalTime = 2;
    [SyncVar(hook = nameof(OnRespawnTimeChange))]
    public float respawnCurrentTime = 0;
    [SyncVar]
    public bool respawning = false;
    Vector3 respawnLocation = Vector3.zero;

    [Header("Inspection")]
    public float inspectionTotalTime = 2;
    [SyncVar(hook = nameof(OnInspectionTimeChange))]
    public float inspectionCurrentTime = 0;
    CursableObject tryingToInspectObject;
    bool inspecting = false;

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

        #region Respawn
        if (respawning)
        {
            respawnCurrentTime += Time.deltaTime;

            if (respawnCurrentTime >= respawnTotalTime)
            {
                ServerCompleteRespawn();
            }
        }
        #endregion

        #region Inspection
        if (inspecting)
        {
            inspectionCurrentTime += Time.deltaTime;

            if (inspectionCurrentTime >= inspectionTotalTime)
            {
                ServerCompleteInspection(tryingToInspectObject);
                ServerStopInspection();
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

    void OnRespawnTimeChange(float oldValue, float newValue)
    {
        if (isLocalPlayer)
            UIManager.singleton.UpdateProgressBar(newValue / respawnTotalTime, ProgressBarType.Respawn);
    }

    void OnInspectionTimeChange(float oldValue, float newValue)
    {
        if (isLocalPlayer)
            UIManager.singleton.UpdateProgressBar(newValue / inspectionTotalTime, ProgressBarType.Inspection);
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
                if (inspecting)
                    ServerStopInspection();
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
            MatchManager.singleton.ServerReduceMatchTime(banishmentMatchTimeReduction);
            ServerStartRespawn();
        }
    }

    [Server]
    void ServerStartRespawn()
    {
        respawning = true;
        respawnLocation = NetManager.singleton.GetSpawnPoint(playerClass).position;
        TargetStartRespawn();
        RpcStartRespawn();
    }

    [TargetRpc]
    void TargetStartRespawn()
    {
        playerMovement.characterController.enabled = false;
    }

    [ClientRpc]
    void RpcStartRespawn()
    {
        ghostMeshRenderer.enabled = false;
    }

    [Server]
    void ServerCompleteRespawn()
    {
        respawning = false;
        respawnCurrentTime = 0;
        TargetCompleteRespawn(respawnLocation);
        RpcCompleteRespawn();
    }

    [TargetRpc]
    void TargetCompleteRespawn(Vector3 respawnLocation)
    {
        playerMovement.characterController.enabled = true;
        transform.position = respawnLocation;
    }

    [ClientRpc]
    void RpcCompleteRespawn()
    {
        ghostMeshRenderer.enabled = true;
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
        if (cursableObject)
        {
            cursableObject.possessingPlayer = this;
        }
        else
        {
            if (possessedObject)
                possessedObject.possessingPlayer = null;
        }

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

    [Server]
    public void ServerStartInspection(CursableObject cursableObject)
    {


        if (inspecting) 
        { 
            ServerStopInspection();
            return;
        }

        //turn lantern off maybe
        inspecting = true;
        tryingToInspectObject = cursableObject;
        TargetStartStopInspection(true);
    }

    [Server]
    void ServerStopInspection()
    {
        //turn lantern on maybe
        inspectionCurrentTime = 0;
        inspecting = false;
        tryingToInspectObject = null;
        TargetStartStopInspection(false);
    }

    [TargetRpc]
    void TargetStartStopInspection(bool start)
    {
        playerMovement.characterController.enabled = !start;
    }

    [Server]
    void ServerCompleteInspection(CursableObject cursableObject)
    {
        //check if the object is being possessed and force unpossess it
        if (cursableObject.possessingPlayer)
        {
            cursableObject.possessingPlayer.ServerEvictPossessingGhost();
        }
    }

    [Server]
    public void ServerEvictPossessingGhost()
    {
        possessedObject.possessingPlayer = null;
        RpcCompletePossession(null);
    }
}