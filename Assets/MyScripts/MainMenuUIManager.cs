using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    public GameObject mainMenuScreen, roomCreationScreen, hostConfigScreen, joinConfigScreen;
    public Button playButton, quitButton, hostConfigButton, joinConfigButton, hostButton, joinButton;
    public Button[] returnButtons;
    public TMP_InputField hostNameInputField, joinNameInputField, joinIPInputField, joinPortInputField;

    NetManager netManager;

    GameObject currentScreen;
    Stack<GameObject> previousScreens = new Stack<GameObject>();

    void Start()
    {
        netManager = NetManager.singleton;
        SteamLobby steamLobby = netManager.GetComponent<SteamLobby>();

        currentScreen = mainMenuScreen;
        mainMenuScreen.SetActive(true);
        roomCreationScreen.SetActive(false);
        hostConfigScreen.SetActive(false);
        joinConfigScreen.SetActive(false);

        playButton.onClick.AddListener(() => OpenScreen(roomCreationScreen));
        hostConfigButton.onClick.AddListener(() => OpenScreen(hostConfigScreen));
        joinConfigButton.onClick.AddListener(() => OpenScreen(joinConfigScreen));
        quitButton.onClick.AddListener(Application.Quit);
        hostButton.onClick.AddListener(steamLobby ? steamLobby.HostLobby : netManager.StartHost);
        joinButton.onClick.AddListener(netManager.StartClient);
        foreach(Button button in returnButtons)
            button.onClick.AddListener(ReturnToPreviousScreen);

        hostNameInputField.text = PlayerPrefs.GetString(NetManager.savekeyPlayerName);
        joinNameInputField.text = PlayerPrefs.GetString(NetManager.savekeyPlayerName);
        hostNameInputField.onValueChanged.AddListener((string s) => PlayerPrefs.SetString(NetManager.savekeyPlayerName, s));
        joinNameInputField.onValueChanged.AddListener((string s) => PlayerPrefs.SetString(NetManager.savekeyPlayerName, s));

        //may need to move this to an OnServerStart or something like that
        joinIPInputField.text = netManager.networkAddress;
        joinIPInputField.onSubmit.AddListener((string text) => netManager.networkAddress = text);
        joinPortInputField.gameObject.SetActive(false);
        if (Transport.active is PortTransport portTransport)
        {
            joinPortInputField.gameObject.SetActive(true);
            joinPortInputField.text = portTransport.Port.ToString();
        }
        joinPortInputField.onSubmit.AddListener(UpdatePort);
    }
    
    void OpenScreen(GameObject newScreen)
    {
        currentScreen.SetActive(false);
        previousScreens.Push(currentScreen);
        currentScreen = newScreen;
        currentScreen.SetActive(true);
    }
    void ReturnToPreviousScreen()
    {
        currentScreen.SetActive(false);
        currentScreen = previousScreens.Pop();
        currentScreen.SetActive(true);
    }
    void UpdatePort(string newPort)
    {
        if (Transport.active is PortTransport portTransport)
        {
            // use TryParse in case someone tries to enter non-numeric characters
            if (ushort.TryParse(newPort, out ushort port))
                portTransport.Port = port;
        }
    }
}