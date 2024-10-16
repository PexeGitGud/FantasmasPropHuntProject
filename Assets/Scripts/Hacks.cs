using UnityEngine;
using UnityEngine.InputSystem;

public class Hacks : MonoBehaviour
{
    [SerializeField]
    bool curseAllObjectsState = false;

    public void CurseAllObjects(InputAction.CallbackContext inputContext)
    {
        if (!inputContext.started)
            return;

        curseAllObjectsState = !curseAllObjectsState;
        foreach (CursableObject co in FindObjectsOfType<CursableObject>())
            co.PlayCursedAnimation(curseAllObjectsState);
    }

    public void Respawn(InputAction.CallbackContext inputContext)
    {
        if(!inputContext.started)
            return;

        foreach(Interactable i in FindObjectsOfType<Interactable>())
            i.CanInteract(false);

        UIManager.singleton.OpenClassSelectionPanel();
        GetComponent<PlayerManager>().DestroyPlayer();
    }

    public void CloseGame(InputAction.CallbackContext inputContext)
    {
        if (!inputContext.started)
            return;

        Application.Quit();
    }
}