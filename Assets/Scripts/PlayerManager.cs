using UnityEngine;
using Mirror;

public enum PlayerClass { Hunter, Ghost };

public class PlayerManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnClassChange))]
    public PlayerClass playerClass;
    public Transform cameraTransform {  get; private set; }
    public MeshRenderer hunterMeshRenderer, ghostMeshRenderer;
    public GameObject hunterLantern;

    PlayerMovement playerMovement;

    public float banishmentTotalTime = 5;
    public float banishmentCurrentTime = 0;
    public float banishmentLastTime = 0;
    bool banishing = false;

    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        playerMovement = GetComponent<PlayerMovement>();
    }

    void LateUpdate()
    {
        if (banishmentCurrentTime > 0) 
        {
            Debug.Log(banishmentCurrentTime);
            if (banishmentCurrentTime >= banishmentTotalTime)
            {
                Debug.Log("BANISHED");
                BanishPlayer();
                return;
            }
            if (banishmentLastTime == banishmentCurrentTime)
            {
                banishmentCurrentTime = Mathf.Max(banishmentCurrentTime - Time.deltaTime, 0f);
                if (banishing)
                {
                    banishing = false;
                    playerMovement.FlashlightSlowdown(false);
                }
            }
            banishmentLastTime = banishmentCurrentTime;
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
                hunterLantern.SetActive(true);
                break;
            case PlayerClass.Ghost:
                tag = "Ghost";
                hunterMeshRenderer.enabled = false;
                ghostMeshRenderer.enabled = true;
                hunterLantern.SetActive(false);
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

    public void FlashlightBanishment()
    {
        banishmentCurrentTime += Time.deltaTime;

        if (!banishing)
        {
            banishing = true;
            playerMovement.FlashlightSlowdown(true);
        }
    }

    void BanishPlayer()
    {
        banishmentCurrentTime = banishmentLastTime = 0;
        playerMovement.FlashlightSlowdown(false);
        transform.position = NetManager.singleton.GetSpawnPoint(playerClass).position;
    }
}