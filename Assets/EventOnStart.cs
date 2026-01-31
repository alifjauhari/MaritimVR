using UnityEngine;
using UnityEngine.Events;

public class EventOnStart : MonoBehaviour
{
    public UnityEvent onStart;
    public bool isWalkthrough = false;

    private void Start()
    {
        if(isWalkthrough)
        {
            onStart?.Invoke();
            return;
        }

        if (PlayerData.Instance.isTugboat)
        {
            return;
        }
        onStart?.Invoke();
    }
}
