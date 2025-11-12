using UnityEngine;

public class CoWorkerIdle : MonoBehaviour
{
    [SerializeField] private Animator coWoekerAnimator;
    [SerializeField] private string idleAnimationName = "CoWorkerIdle";

    public void PlayIdleAnimation()
    {
        coWoekerAnimator.Play(idleAnimationName);
    }
}
