using Mirror;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SpawnPoint : NetworkBehaviour
{
    public PlayerClass playerClass = PlayerClass.Hunter;
    public float cooldownTime = 10;

    public void Awake()
    {
        NetManager.RegisterSpawnPoint(this);
    }

    public void OnDestroy()
    {
        NetManager.UnRegisterSpawnPoint(this);
    }

    public void StartCooldown()
    {
        if (isServer)
            StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        NetManager.UnRegisterSpawnPoint(this);
        yield return new WaitForSeconds(cooldownTime);
        NetManager.RegisterSpawnPoint(this);
    }
}