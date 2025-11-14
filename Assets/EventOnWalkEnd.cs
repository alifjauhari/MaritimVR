using UnityEngine;
using UnityEngine.Events;

public class EventOnWalkEnd : MonoBehaviour
{
    public UnityEvent onFinishEngine2Ae;
    public UnityEvent onFinishEngine3Me;
    public UnityEvent onFinishEngine4RPM;
    public UnityEvent onFinishEngine5Dt;

    public void FinishEngine2Ae()
    {
        onFinishEngine2Ae.Invoke();
    }

    public void FinishEngine3Me()
    {
        onFinishEngine3Me.Invoke();
    }

    public void FinishEngine4RPM()
    {
        onFinishEngine4RPM.Invoke();
    }

    public void FinishEngine5Dt()
    {
        onFinishEngine5Dt.Invoke();
    }
}
