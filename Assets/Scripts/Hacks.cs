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
}