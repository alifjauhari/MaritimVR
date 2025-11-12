using UnityEngine;
using UnityEngine.Events;

public class EventOnStart : MonoBehaviour
{
    public UnityEvent onStart;

    private void Start()
    {
        if (PlayerData.Instance.isTugboat)
        {
            return;
        }
        onStart?.Invoke();
    }
}
