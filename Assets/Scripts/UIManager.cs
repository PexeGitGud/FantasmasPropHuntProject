using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Singleton
    public static UIManager singleton { get; internal set; }

    void Awake()
    {
        if (singleton)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;
    }
    #endregion

    public GameObject classSelectionCamera, classSelectionPanel, playerUI, gameOverScreen;
    public Button ghostButton, hunterButton, returnToLobbyButton, quitButton;
    public TMP_Text playerClassText, matchTimer;
    public Material outlineMaterial;
    public Color ghostColor, hunterColor;

    void Start()
    {
        ghostButton.onClick.AddListener(SpawnGhostButton);
        hunterButton.onClick.AddListener(SpawnHunterButton);
        returnToLobbyButton.onClick.AddListener(NetManager.singleton.ReturnToLobby);
        quitButton.onClick.AddListener(NetManager.singleton.ExitRoom);

        CloseClassSelectionPanel();
        gameOverScreen.SetActive(false);
    }

    void SpawnGhostButton()
    {
        if (!NetworkClient.active) return;

        NetManager.CreatePlayerMessage createPlayerMessage = new NetManager.CreatePlayerMessage
        {
            playerClass = PlayerClass.Ghost
        };
        NetManager.singleton.CreatePlayer(createPlayerMessage);

        CloseClassSelectionPanel();
        ChangeClassUI(PlayerClass.Ghost);
    }

    void SpawnHunterButton()
    {
        if (!NetworkClient.active) return;

        NetManager.CreatePlayerMessage createPlayerMessage = new NetManager.CreatePlayerMessage
        {
            playerClass = PlayerClass.Hunter
        };
        NetManager.singleton.CreatePlayer(createPlayerMessage);

        CloseClassSelectionPanel();
        ChangeClassUI(PlayerClass.Hunter);
    }

    public void ChangeClassUI(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Hunter:
                playerClassText.SetText("Hunter");
                playerClassText.color = hunterColor;
                outlineMaterial.SetColor("_Outline_Color", hunterColor);
                break;
            case PlayerClass.Ghost:
                playerClassText.SetText("Ghost");
                playerClassText.color = ghostColor;
                outlineMaterial.SetColor("_Outline_Color", ghostColor);
                break;
        }
    }

    void CloseClassSelectionPanel()
    {
        classSelectionCamera.SetActive(false);
        classSelectionPanel.SetActive(false);
        playerUI.SetActive(true);
    }

    public void OpenClassSelectionPanel()
    {
        Cursor.lockState = CursorLockMode.None;
        classSelectionCamera.SetActive(true);
        classSelectionPanel.SetActive(true);
        playerUI.SetActive(false);
        playerClassText.SetText("");
    }

    public void UpdateMatchTimeText(float matchTime)
    {
        TimeSpan time = TimeSpan.FromSeconds(MathF.Max(matchTime, 0));
        matchTimer.text = time.ToString("mm':'ss");
    }

    public void ShowGameOverScreen()
    {
        gameOverScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }
}