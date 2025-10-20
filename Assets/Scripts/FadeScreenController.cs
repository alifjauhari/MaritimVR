using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FadeScreenController : MonoBehaviour
{
    [SerializeField] private FadeScreen fadeScreenMainCamera;
    [SerializeField] private FadeScreen fadeScreenStaticCamera;

    [SerializeField] private UnityEvent onCameraMain;
    [SerializeField] private UnityEvent onCameraStatic;

    public void ChangeStaticCamera()
    {
        StartCoroutine(ChangeStaticCameraIEnumerator());
    }

    public void ChangeMainCamera()
    {
        StartCoroutine(ChangeMainCameraIEnumerator());
    }

    private IEnumerator ChangeStaticCameraIEnumerator()
    {
        fadeScreenMainCamera.FadeOut();
        yield return new WaitForSeconds(fadeScreenMainCamera.duration);
        onCameraStatic?.Invoke();
        fadeScreenStaticCamera.FadeIn();
    }

    private IEnumerator ChangeMainCameraIEnumerator()
    {
        fadeScreenStaticCamera.FadeOut();
        yield return new WaitForSeconds(fadeScreenStaticCamera.duration);
        onCameraMain?.Invoke();
        fadeScreenMainCamera.FadeIn();
    }
}
