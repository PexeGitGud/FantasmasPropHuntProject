using DG.Tweening;
using UnityEngine;

public class CursableObject : MonoBehaviour
{
    public bool cursed = false;
    Vector3 ogPos;

    void Start()
    {
        DOTween.Init();
        ogPos = transform.position;
    }

    public void Interact(PlayerManager player)
    {
        switch (player.playerClass)
        {
            case PlayerClass.Hunter:
                //PlayCursedAnimation(false);
                break;
            case PlayerClass.Ghost:
                player.PossessCursableObject(this);
                break;
        }
    }

    public void PlayCursedAnimation(bool play = true)
    {
        cursed = play;
        if (cursed)
        {
            transform.DOMoveY(ogPos.y + .25f, .5f).SetEase(Ease.InOutSine).OnComplete(()=>transform.DOMoveY(ogPos.y + .75f, .75f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));
            transform.DOShakeRotation(5, 10, 1, 90, false, ShakeRandomnessMode.Full).SetRelative().SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Incremental);
            MatchManager.singleton.CurseCallback();
            return;
        }

        transform.DOKill();
        transform.DOMoveY(ogPos.y,.5f).SetEase(Ease.InOutSine);
        transform.DORotate(Vector3.zero, .5f).SetEase(Ease.InOutSine);
    }
}