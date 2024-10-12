using UnityEngine;
using Mirror;

public enum PlayerClass { Hunter, Ghost };

public class PlayerManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnClassChange))]
    public PlayerClass playerClass;
    public Transform cameraTransform {  get; private set; }
    public MeshRenderer hunterMeshRenderer, ghostMeshRenderer;

    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    void OnClassChange(PlayerClass oldClass, PlayerClass newClass)
    {
        switch (newClass)
        {
            case PlayerClass.Hunter:
                hunterMeshRenderer.enabled = true;
                ghostMeshRenderer.enabled = false;
                break;
            case PlayerClass.Ghost:
                hunterMeshRenderer.enabled = false;
                ghostMeshRenderer.enabled = true;
                break;
        }
    }
}