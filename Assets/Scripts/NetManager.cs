using Mirror;
using UnityEngine;

public class NetManager : NetworkManager
{
    public static new NetManager singleton => (NetManager)NetworkManager.singleton;

    public struct CreatePlayerMessage : NetworkMessage
    {
        public bool isGhostClass;
    }

    //[Header("Custom")]

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }

    void OnCreatePlayer(NetworkConnectionToClient conn, CreatePlayerMessage message)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);
        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

        player.GetComponent<PlayerManager>().isGhostClass = message.isGhostClass;

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public void CreatePlayer(CreatePlayerMessage message) 
    {
        NetworkClient.Send(message);
    }
}