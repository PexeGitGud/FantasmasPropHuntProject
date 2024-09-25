using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    [SerializeField]
    PlayerManager playerPrefab;
    [SerializeField]
    Material outlineMaterial;

    [SerializeField]
    GameObject classSelectionCamera;
    [SerializeField]
    GameObject classSelectionPanel;
    [SerializeField]
    GameObject playerUI;
    [SerializeField]
    TMP_Text playerClassText;

    [SerializeField]
    Color ghostColor;
    [SerializeField]
    Color hunterColor;

    SpawnPoint[] spawnPoints;

    void Start()
    {
        playerUI.SetActive(false);

        spawnPoints = FindObjectsOfType<SpawnPoint>();
    }

    public void SpawnGhost()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
        PlayerManager spawnedPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedPlayer.SelectClass(true);
        classSelectionCamera.SetActive(false);
        classSelectionPanel.SetActive(false);
        playerUI.SetActive(true);
        playerClassText.SetText("Ghost");
        playerClassText.color = ghostColor;
        outlineMaterial.SetColor("_Outline_Color", ghostColor);
    }

    public void SpawnHunter()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
        PlayerManager spawnedPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedPlayer.SelectClass(false);
        classSelectionCamera.SetActive(false);
        classSelectionPanel.SetActive(false);
        playerUI.SetActive(true);
        playerClassText.SetText("Hunter");
        playerClassText.color = hunterColor;
        outlineMaterial.SetColor("_Outline_Color", hunterColor);
    }

    public void ShowPlayerSelection()
    {
        Cursor.lockState = CursorLockMode.None;
        classSelectionCamera.SetActive(true);
        classSelectionPanel.SetActive(true);
        playerUI.SetActive(false);
        playerClassText.SetText("");
    }
}