using Mirror;
using UnityEngine;

public class RoomPlayer : NetworkRoomPlayer
{
    [ReadOnly]
    [SyncVar(hook = nameof(PlayerNameChanged))]
    public string playerName;
    [ReadOnly]
    [SyncVar(hook = nameof(PlayerClassChanged))]
    public PlayerClass playerClass;
    [ReadOnly]
    [SyncVar]
    public bool playerHost;

    public RoomPlayerUI roomPlayerUIPrefab;
    RoomPlayerUI myRoomPlayerUI;

    RoomUIManager roomUIManager;

    bool clientStarted = false;

    public override void OnStartClient()
    {
        CreateMyRoomPlayerUI();
        clientStarted = true;

        base.OnStartClient();
    }

    void CreateMyRoomPlayerUI()
    {
        roomUIManager = FindFirstObjectByType<RoomUIManager>();

        myRoomPlayerUI = Instantiate(roomPlayerUIPrefab, roomUIManager.roomPlayerList);

        myRoomPlayerUI.classText.text = playerClass.ToString();
        myRoomPlayerUI.classDropdown.value = (int)playerClass;
        myRoomPlayerUI.readyToggle.isOn = readyToBegin;

        if (isLocalPlayer)
        {
            if (NetworkClient.ready)
                CmdChangeName(PlayerPrefs.GetString(NetManager.savekeyPlayerName));
            else
                myRoomPlayerUI.nameText.text = playerName;

            myRoomPlayerUI.classText.gameObject.SetActive(false);
            myRoomPlayerUI.classDropdown.gameObject.SetActive(true);
            myRoomPlayerUI.classDropdown.onValueChanged.AddListener((int pc) => CmdChangeClass((PlayerClass)pc));
            myRoomPlayerUI.readyToggle.interactable = true;
            myRoomPlayerUI.readyToggle.onValueChanged.AddListener((bool b) => CmdChangeReadyState(b));

            if (isServer)
            {
                playerHost = true;
                myRoomPlayerUI.kickPlayerButton.onClick.AddListener(GetComponent<NetworkIdentity>().connectionToClient.Disconnect);
            }
        }
        else
        {
            myRoomPlayerUI.nameText.text = playerName;

            if (isServer)
            {
                myRoomPlayerUI.kickPlayerButton.gameObject.SetActive(true);
                myRoomPlayerUI.kickPlayerButton.onClick.AddListener(GetComponent<NetworkIdentity>().connectionToClient.Disconnect);
            }
        }

        myRoomPlayerUI.hostText.SetActive(playerHost);
    }

    [Command]
    void CmdChangeName(string name)
    {
        playerName = name;
    }

    [Command]
    void CmdChangeClass(PlayerClass pc)
    {
        playerClass = pc;
    }

    public override void IndexChanged(int oldIndex, int newIndex)
    {

    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        if (myRoomPlayerUI)
            myRoomPlayerUI.readyToggle.isOn = newReadyState;
    }

    public void PlayerNameChanged(string oldName, string newName)
    {
        if (myRoomPlayerUI)
            myRoomPlayerUI.nameText.text = newName;
    }

    public void PlayerClassChanged(PlayerClass oldClass, PlayerClass newClass)
    {
        if (myRoomPlayerUI)
            myRoomPlayerUI.classText.text = newClass.ToString();
    }

    public override void OnClientEnterRoom()
    {
        if (!myRoomPlayerUI && clientStarted)
            CreateMyRoomPlayerUI();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (myRoomPlayerUI)
            Destroy(myRoomPlayerUI.gameObject);
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }
}