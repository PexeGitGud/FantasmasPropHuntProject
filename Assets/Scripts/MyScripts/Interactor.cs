using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class Interactor : NetworkBehaviour
{
    PlayerManager playerManager;

    [SerializeField]
    LayerMask interactionLayerMask;
    [SerializeField]
    float interactionRange = 5;
    [SerializeField]
    float interactionRadius = .5f;

    Interactable lastInteractable;

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    void FixedUpdate()
    {
        if (!playerManager.cameraTransform) return;

        Ray r = new Ray(playerManager.cameraTransform.position, playerManager.cameraTransform.forward);
        RaycastHit hit = new RaycastHit();

        if (Physics.SphereCast(r, interactionRadius, out hit, interactionRange, interactionLayerMask.value))
        {
            Interactable newInteractableHit = hit.transform.GetComponent<Interactable>();
            if (newInteractableHit)
            {
                if (lastInteractable && lastInteractable == newInteractableHit)
                    return;

                if (lastInteractable != newInteractableHit)
                    lastInteractable?.CanInteract(false);

                lastInteractable = newInteractableHit;
                lastInteractable.CanInteract(true);
            }
        }
        else
        {
            lastInteractable?.CanInteract(false);
            lastInteractable = null;
            return;
        }
    }

    public void PlayerMainInteraction(InputAction.CallbackContext inputContext)
    {
        if (isLocalPlayer && inputContext.started)
            CmdPlayerMainInteraction();
    }

    [Command]
    void CmdPlayerMainInteraction()
    {
        if (lastInteractable && !lastInteractable.disabled)
        {
            lastInteractable.ServerInteract(playerManager);
            return;
        }
        playerManager.ServerInteractionNull();
    }

    private void OnDrawGizmosSelected()
    {
        if (!playerManager)
            return;

        Ray r = new Ray(playerManager.cameraTransform.position, playerManager.cameraTransform.forward);
        RaycastHit hit = new RaycastHit();
        Gizmos.color = Physics.SphereCast(r, interactionRadius, out hit, interactionRange, interactionLayerMask.value) ? Color.green : Color.red;
        Gizmos.DrawLine(playerManager.cameraTransform.position, playerManager.cameraTransform.position + playerManager.cameraTransform.forward * interactionRange);
        Gizmos.DrawWireSphere(playerManager.cameraTransform.position, interactionRadius);
        Gizmos.DrawWireSphere(playerManager.cameraTransform.position + playerManager.cameraTransform.forward * interactionRange, interactionRadius);
    }
}