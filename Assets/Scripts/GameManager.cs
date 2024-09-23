using UnityEngine;

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
    GameObject classSelectionCamera;
    [SerializeField]
    GameObject classSelectionPanel;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnGhost()
    {
        PlayerManager spawnedPlayer = Instantiate(playerPrefab);
        spawnedPlayer.SelectClass(true);
        classSelectionCamera.SetActive(false);
        classSelectionPanel.SetActive(false);
    }

    public void SpawnHunter()
    {
        PlayerManager spawnedPlayer = Instantiate(playerPrefab);
        spawnedPlayer.SelectClass(false);
        classSelectionCamera.SetActive(false);
        classSelectionPanel.SetActive(false);
    }

    public void ShowPlayerSelection()
    {
        Cursor.lockState = CursorLockMode.None;
        classSelectionCamera.SetActive(true);
        classSelectionPanel.SetActive(true);
    }
}