using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetManager : NetworkManager
{
    public static new NetManager singleton => (NetManager)NetworkManager.singleton;

    public struct CreatePlayerMessage : NetworkMessage
    {
        public PlayerClass playerClass;
    }

    [Header("Custom")]
    public static List<SpawnPoint> hunterSpawnPoints = new List<SpawnPoint>();
    public static List<SpawnPoint> ghostSpawnPoints = new List<SpawnPoint>();

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }
    
    void OnCreatePlayer(NetworkConnectionToClient conn, CreatePlayerMessage message)
    {
        Transform startPos = GetSpawnPoint(message.playerClass);
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

        player.GetComponent<PlayerManager>().playerClass = message.playerClass;

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public void CreatePlayer(CreatePlayerMessage message) 
    {
        NetworkClient.Send(message);
    }

    public static void RegisterSpawnPoint(SpawnPoint spawnPoint)
    {
        switch (spawnPoint.playerClass)
        {
            case PlayerClass.Hunter:
                hunterSpawnPoints.Add(spawnPoint);
                break;
            case PlayerClass.Ghost:
                ghostSpawnPoints.Add(spawnPoint);
                break;
        }
    }

    public static void UnRegisterSpawnPoint(SpawnPoint spawnPoint)
    {
        switch (spawnPoint.playerClass)
        {
            case PlayerClass.Hunter:
                hunterSpawnPoints.Remove(spawnPoint);
                break;
            case PlayerClass.Ghost:
                ghostSpawnPoints.Remove(spawnPoint);
                break;
        }
    }

    public virtual Transform GetSpawnPoint(PlayerClass playerClass)
    {
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

        switch (playerClass)
        {
            case PlayerClass.Hunter:
                spawnPoints = hunterSpawnPoints;
                break;
            case PlayerClass.Ghost:
                spawnPoints = ghostSpawnPoints;
                break;
        }

        // first remove any dead transforms
        spawnPoints.RemoveAll(t => t == null);

        if (spawnPoints.Count == 0)
            return null;

        SpawnPoint spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        spawnPoint.StartCoroutine(spawnPoint.Cooldown(5));
        return spawnPoint.transform;
    }
}