using UnityEngine;
using UnityEngine.Events;
public class ColliderTrigger : MonoBehaviour
{
    public UnityEvent onTriggerEnter;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            onTriggerEnter.Invoke();
    }
}
