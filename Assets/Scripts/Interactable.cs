using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
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

    void Start()
    {
        outlineableObject.layer = defaultLayerInt;
    }

    public void CanInteract(bool value)
    {
        outlineableObject.layer = value ? outlineLayerInt : defaultLayerInt;
    }

    public void Interact(bool isGhostClass)
    {
        if (isGhostClass)
            ghostInteractionEvent.Invoke();
        else
            hunterInteractionEvent.Invoke();
    }
}