using Mirror;
using System.Collections;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    #region Singleton
    public static MatchManager singleton;
    private void Awake()
    {
        if (singleton)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;
    }
    #endregion

    public float matchTotalTime = 1*60;
    [SyncVar(hook = nameof(OnMatchTimeUpdate)), ReadOnly]
    public float matchTime = -1;

    void Start()
    {
        if (isServer) ServerStartMatch();
    }

    [Server]
    public void ServerStartMatch()
    {
        matchTime = matchTotalTime;
        StartCoroutine(MatchTimeUpdate());
    }

    IEnumerator MatchTimeUpdate()
    {
        while (matchTime >= 0)
        {
            yield return new WaitForSeconds(1);
            matchTime -= 1;
        }

        ServerEndMatch();
    }

    [Server]
    void ServerEndMatch()
    {
        UIManager.singleton.ShowGameOverScreen();
        RpcEndMatch();
    }

    [ClientRpc]
    void RpcEndMatch()
    {
        UIManager.singleton.ShowGameOverScreen();
    }

    public void OnMatchTimeUpdate(float oldTime, float newTime)
    {
        UIManager.singleton.UpdateMatchTimeText(newTime);
    }

    [Server]
    public void ServerReduceMatchTime(float time)
    {
        matchTime -= time;
    }
}