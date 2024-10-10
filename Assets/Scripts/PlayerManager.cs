using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnClassChange))]
    public bool isGhostClass;
    public Transform cameraTransform {  get; private set; }
    public MeshRenderer hunterMeshRenderer, ghostMeshRenderer;

    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void OnClassChange(bool oldClass, bool newClass)
    {
        ghostMeshRenderer.enabled = newClass;
        hunterMeshRenderer.enabled = !newClass;
    }
}