using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetManager : NetworkRoomManager
{
    public static new NetManager singleton => NetworkManager.singleton as NetManager;

    public static string savekeyPlayerName = "localPlayer";

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

        //NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }
    public override void OnRoomServerPlayersReady()
    {
        if (Utils.IsSceneActive(RoomScene))
            FindFirstObjectByType<RoomUIManager>().startGameButton.interactable = true;
    }

    public override void OnRoomServerPlayersNotReady()
    {
        if (Utils.IsSceneActive(RoomScene))
            FindFirstObjectByType<RoomUIManager>().startGameButton.interactable = false;
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        RoomPlayer rp = roomPlayer.GetComponent<RoomPlayer>();
        Transform startPos = GetSpawnPoint(rp.playerClass);
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

        player.GetComponent<PlayerManager>().playerClass = rp.playerClass;
        switch (rp.playerClass)
        {
            case PlayerClass.Hunter:
                player.tag = "Hunter";
                break;
            case PlayerClass.Ghost:
                player.tag = "Ghost";
                break;
        }
        return player;
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
        switch (message.playerClass)
        {
            case PlayerClass.Hunter:
                player.tag = "Hunter";
                break;
            case PlayerClass.Ghost:
                player.tag = "Ghost";
                break;
        }

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
        spawnPoint.StartCooldown();
        return spawnPoint.transform;
    }

    public void ReturnToLobby()
    {
        if (NetworkClient.activeHost)
            ServerChangeScene(RoomScene);
    }

    public void ExitRoom()
    {
        if (NetworkClient.activeHost)
        {
            StopHost();
            return;
        }
        StopClient();
    }
}