using UnityEngine;
using UnityEngine.Events;
public class ColliderTrigger : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            onTriggerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            onTriggerExit.Invoke();
    }
}
