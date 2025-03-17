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

    void Start()
    {
        CanInteract(false);

        CursableObject cursableObject = GetComponent<CursableObject>();
        if (cursableObject)
        {
            interactionEvent.AddListener(cursableObject.Interact);
        }
    }

    public void CanInteract(bool value)
    {
        outlineableObject.layer = value ? outlineLayerInt : defaultLayerInt;
        for (int i = 0; i < outlineableObject.transform.childCount; i++)
        {
            outlineableObject.transform.GetChild(i).gameObject.layer = outlineableObject.layer;
        }
    }

    [ClientRpc]
    public void RpcInteract(PlayerManager player)
    {
        interactionEvent.Invoke(player);
    }
}