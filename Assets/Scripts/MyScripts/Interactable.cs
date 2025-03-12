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

    public UnityEvent<PlayerManager> interactionEvent;

    public Material tvScreen;

    void Start()
    {
        outlineableObject.layer = defaultLayerInt;

        CursableObject cursableObject = GetComponent<CursableObject>();
        if (cursableObject)
        {
            interactionEvent.AddListener(cursableObject.Interact);
        }
        if (tvScreen)
        {
            //ghostInteractionEvent.AddListener(() => tvScreen.EnableKeyword("_EMISSION"));
            //butlerInteractionEvent.AddListener(() => tvScreen.DisableKeyword("_EMISSION"));
        }
    }

    public void CanInteract(bool value)
    {
        outlineableObject.layer = value ? outlineLayerInt : defaultLayerInt;
    }

    [ClientRpc]
    public void RpcInteract(PlayerManager player)
    {
        interactionEvent.Invoke(player);
    }
}