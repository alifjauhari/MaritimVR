using UnityEngine;
using UnityEngine.Events;

public class AnimationEventController : MonoBehaviour
{
    public UnityEvent onAnimationEnd;
    private bool first;
    private bool second;
    public UnityEvent onAnimationEndAgain;

    private void Start()
    {
        first = true;
        second = false;
    }

    public void AnimationEnded()
    {
        if (first)
        {
            onAnimationEnd.Invoke();
            first = false;
            second = true;
        }
        else if (second)
        {
            onAnimationEndAgain.Invoke();
        }
    }
}
