using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public enum PlayerClass { Butler, Ghost };

public class PlayerManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnClassChange))]
    public PlayerClass playerClass;
    public Transform cameraTransform {  get; private set; }
    public MeshRenderer butlerMeshRenderer, ghostMeshRenderer;
    public ButlerFlashlight butlerFlashlight;

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
        RpcToggleFlashlight(!butlerFlashlight.lightOn);
    }

    [ClientRpc]
    void RpcToggleFlashlight(bool value)
    {
        butlerFlashlight.ToggleFlashlight(value);
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

    public void PossessCursableObject(CursableObject cursableObject)
    {
        //if(cameraTransform)
        //    cameraTransform.gameObject.SetActive(false);

        cursableObject.PlayCursedAnimation(true);
    }
}