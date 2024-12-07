using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Interactable : NetworkBehaviour
{
    [SerializeField]
    GameObject outlineableObject;
    [SerializeField]
    int defaultLayerInt;
    [SerializeField]
    int outlineLayerInt;

    [SerializeField]
    UnityEvent ghostInteractionEvent;
    [SerializeField]
    UnityEvent hunterInteractionEvent;

    public Material tvScreen;

    void Start()
    {
        outlineableObject.layer = defaultLayerInt;
        if (tvScreen)
        {
            ghostInteractionEvent.AddListener(() => tvScreen.EnableKeyword("_EMISSION"));
            hunterInteractionEvent.AddListener(() => tvScreen.DisableKeyword("_EMISSION"));
        }
    }

    public void CanInteract(bool value)
    {
        outlineableObject.layer = value ? outlineLayerInt : defaultLayerInt;
    }

    [ClientRpc]
    public void RpcInteract(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Hunter:
                hunterInteractionEvent.Invoke();
                break;
            case PlayerClass.Ghost:
                ghostInteractionEvent.Invoke();
                break;
        }
    }
}