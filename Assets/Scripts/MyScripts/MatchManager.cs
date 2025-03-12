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

    CursableObject[] cursableObjects;
    bool allCursed = false;

    void Start()
    {
        if (isServer) ServerStartMatch();

        cursableObjects = FindObjectsOfType<CursableObject>();
    }

    [Server]
    public void ServerStartMatch()
    {
        matchTime = matchTotalTime;
        StartCoroutine(MatchTimeUpdate());
    }

    IEnumerator MatchTimeUpdate()
    {
        while (matchTime >= 0 && !allCursed)
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

    public void CurseCallback()
    {
        int cursedAmount = 0;
        foreach (CursableObject cursable in cursableObjects)
            cursedAmount += cursable.cursed ? 1 : 0;
        UIManager.singleton.UpdateMatchSpookOMeter(cursedAmount, cursableObjects.Length);

        if (isServer)
            ServerCurseCallback();
    }

    [Server]
    void ServerCurseCallback()
    {
        allCursed = CheckIfAllCursed();
    }

    bool CheckIfAllCursed()
    {
        foreach (CursableObject cursable in cursableObjects)
        {
            if (!cursable.cursed)
                return false;
        }
        return true;
    }
}