using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SpawnPoint : MonoBehaviour
{
    public PlayerClass playerClass = PlayerClass.Hunter;

    public void Awake()
    {
        NetManager.RegisterSpawnPoint(this);
    }

    public void OnDestroy()
    {
        NetManager.UnRegisterSpawnPoint(this);
    }

    public IEnumerator Cooldown(float sec)
    {
        NetManager.UnRegisterSpawnPoint(this);
        yield return new WaitForSeconds(sec);
        NetManager.RegisterSpawnPoint(this);
    }
}