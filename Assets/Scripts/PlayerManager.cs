using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public enum PlayerClass { Hunter, Ghost };

public class PlayerManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnClassChange))]
    public PlayerClass playerClass;
    public Transform cameraTransform {  get; private set; }
    public MeshRenderer hunterMeshRenderer, ghostMeshRenderer;
    public HunterFlashlight hunterFlashlight;

    PlayerMovement playerMovement;

    public float banishmentTotalTime = 2;
    public float banishmentCurrentTime = 0;
    public float banishmentLastTime = 0;
    public float banishmentMatchTimeReduction = 5;
    bool banishing = false;

    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        playerMovement = GetComponent<PlayerMovement>();

        if (isLocalPlayer)
            FindFirstObjectByType<UIManager>()?.ChangeClassUI(playerClass);
    }

    void LateUpdate()
    {
        if (!isServer) return;

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
                    playerMovement.ServerFlashlightSlowdown(false);
                }
            }
            banishmentLastTime = banishmentCurrentTime;
        }
        if (banishmentCurrentTime < 0)
        {
            banishmentCurrentTime = Mathf.Min(banishmentCurrentTime + Time.deltaTime, 0f);
        }
    }

    void OnClassChange(PlayerClass oldClass, PlayerClass newClass)
    {
        switch (newClass)
        {
            case PlayerClass.Hunter:
                tag = "Hunter";
                hunterMeshRenderer.enabled = true;
                ghostMeshRenderer.enabled = false;
                hunterFlashlight.gameObject.SetActive(true);
                break;
            case PlayerClass.Ghost:
                tag = "Ghost";
                hunterMeshRenderer.enabled = false;
                ghostMeshRenderer.enabled = true;
                hunterFlashlight.gameObject.SetActive(false);
                break;
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

    public void ToggleFlashlight(InputAction.CallbackContext inputContext)
    {
        if (!inputContext.started)
            return;

        CmdToggleFlashlight();
    }

    [Command]
    void CmdToggleFlashlight()
    {
        RpcToggleFlashlight(!hunterFlashlight.lightOn);
    }

    [ClientRpc]
    void RpcToggleFlashlight(bool value)
    {
        hunterFlashlight.ToggleFlashlight(value);
    }

    [Server]
    public void ServerFlashlightBanishment()
    {
        banishmentCurrentTime += Time.deltaTime;

        if (!banishing && banishmentCurrentTime >= 0)
        {
            banishing = true;
            playerMovement.ServerFlashlightSlowdown(true);
        }
    }

    [Server]
    void ServerBanishPlayer()
    {
        banishmentLastTime = 0;
        banishmentCurrentTime = -3 * banishmentTotalTime;
        banishing = false;
        playerMovement.ServerFlashlightSlowdown(false);
        playerMovement.respawning = true;
        TargetRespawn(NetManager.singleton.GetSpawnPoint(playerClass).position);
        MatchManager.singleton.ServerReduceMatchTime(banishmentMatchTimeReduction);
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
}