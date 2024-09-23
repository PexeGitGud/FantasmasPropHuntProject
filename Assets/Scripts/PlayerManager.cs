using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public bool isGhostClass { get; private set; } = false;

    public Transform cameraTransform {  get; private set; }

    [SerializeField]
    GameObject ghostVisuals;
    [SerializeField]
    GameObject hunterVisuals;

    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SelectClass(bool isGhostClass)
    {
        this.isGhostClass = isGhostClass;
        Instantiate(isGhostClass ? ghostVisuals : hunterVisuals, transform);
    }
}