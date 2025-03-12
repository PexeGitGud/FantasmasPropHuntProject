using Mirror;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SpawnPoint : NetworkBehaviour
{
    public PlayerClass playerClass = PlayerClass.Butler;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + transform.forward * .1f, transform.position + transform.forward * .3f);
        Gizmos.DrawWireCube(transform.position, Vector3.one * .2f);
    }
}