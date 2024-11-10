using Mirror;
using UnityEngine;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(PlayerNameChanged)), ReadOnly]
    public string playerName;
    [SyncVar(hook = nameof(PlayerClassChanged)), ReadOnly]
    public PlayerClass playerClass;
    [SyncVar, ReadOnly]
    public bool playerHost;

    public RoomPlayerUI roomPlayerUIPrefab;
    RoomPlayerUI myRoomPlayerUI;

    RoomUIManager roomUIManager;

    public override void Start()
    {
        roomUIManager = FindFirstObjectByType<RoomUIManager>();

        myRoomPlayerUI = Instantiate(roomPlayerUIPrefab, roomUIManager.roomPlayerList);

        if (isLocalPlayer)
        {
            CmdChangeName(PlayerPrefs.GetString(NetManager.savekeyPlayerName));
            myRoomPlayerUI.classText.gameObject.SetActive(false);
            myRoomPlayerUI.classDropdown.gameObject.SetActive(true);
            myRoomPlayerUI.classDropdown.onValueChanged.AddListener((int pc) => CmdChangeClass((PlayerClass)pc));
            myRoomPlayerUI.readyToggle.interactable = true;
            myRoomPlayerUI.readyToggle.onValueChanged.AddListener((bool b) => CmdChangeReadyState(b));
            myRoomPlayerUI.kickPlayerButton.onClick.AddListener(GetComponent<NetworkIdentity>().connectionToClient.Disconnect);

            if (isServer)
            {
                playerHost = true;
            }
        }
        else
        {
            if (isServer)
            {
                myRoomPlayerUI.kickPlayerButton.gameObject.SetActive(true);
                myRoomPlayerUI.kickPlayerButton.onClick.AddListener(GetComponent<NetworkIdentity>().connectionToClient.Disconnect);
            }
        }


        base.Start();
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
        myRoomPlayerUI.readyToggle.isOn = newReadyState;
    }

    public void PlayerNameChanged(string oldName, string newName)
    {
        myRoomPlayerUI.nameText.text = newName;
    }

    public void PlayerClassChanged(PlayerClass oldClass, PlayerClass newClass)
    {
        myRoomPlayerUI.classText.text = newClass.ToString();
    }

    public override void OnClientEnterRoom()
    {
        myRoomPlayerUI.nameText.text = playerName;
        myRoomPlayerUI.hostText.SetActive(playerHost);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        Destroy(myRoomPlayerUI.gameObject);
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }
}