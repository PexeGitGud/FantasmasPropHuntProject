using UnityEngine;
using UnityEngine.UI;

public class RoomUIManager : MonoBehaviour
{
    public Transform roomPlayerList;
    public Button startGameButton, exitRoomButton;

    void Start()
    {
        NetManager netManager = NetManager.singleton;
        startGameButton.onClick.AddListener(() => netManager.ServerChangeScene(netManager.GameplayScene));
        exitRoomButton.onClick.AddListener(netManager.ExitRoom);
    }
}