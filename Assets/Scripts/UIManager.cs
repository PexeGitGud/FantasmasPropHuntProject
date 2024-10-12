using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject classSelectionCamera, classSelectionPanel, playerUI;
    public Button ghostButton, hunterButton;
    public TMP_Text playerClassText;
    public Material outlineMaterial;
    public Color ghostColor, hunterColor;

    void Start()
    {
        ghostButton.onClick.AddListener(SpawnGhostButton);
        hunterButton.onClick.AddListener(SpawnHunterButton);
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
        playerClassText.SetText("Ghost");
        playerClassText.color = ghostColor;
        outlineMaterial.SetColor("_Outline_Color", ghostColor);
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
        playerClassText.SetText("Hunter");
        playerClassText.color = hunterColor;
        outlineMaterial.SetColor("_Outline_Color", hunterColor);
    }

    void CloseClassSelectionPanel()
    {
        classSelectionCamera.SetActive(false);
        classSelectionPanel.SetActive(false);
        playerUI.SetActive(true);
    }
}