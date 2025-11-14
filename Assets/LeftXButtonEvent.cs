using UnityEngine;
using UnityEngine.Events;

public class LeftXButtonEvent : MonoBehaviour
{
    public UnityEvent onLeftXDown;

    void Update()
    {
        // X di kontroler kiri = Button.One dengan controller LTouch
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            Debug.Log("Tombol X kiri ditekan");
            onLeftXDown?.Invoke();
        }
    }
}
