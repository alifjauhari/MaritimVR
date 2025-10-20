using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class FadeScreen : MonoBehaviour
{
    public enum SkyBoxType
    {
        Space,
    }

    public Material spaceSkyBox;

    public Camera centerEye;

    public SkyBoxType skyBox = SkyBoxType.Space;
    public bool fadeonStart = false;
    public float duration = 2;
    public Color fadeColor;
    private Renderer renderer;

    [SerializeField] UnityEvent fadeInSpace;
    [SerializeField] UnityEvent fadeOutSpace;

    private void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        renderer = GetComponent<Renderer>();
        if (fadeonStart)
        {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        Fade(1, 0);
    }

    public void FadeOut()
    {
        Fade(0, 1);
    }

    public void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    public IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;

        float timer = 0;
        while (timer < duration)
        {
            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer/duration);

            renderer.material.SetColor("_Color", newColor);

            timer += Time.deltaTime;
            yield return null;
        }

        Color newColor2 = fadeColor;
        newColor2.a = alphaOut;
        renderer.material.SetColor("_Color", newColor2);

        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void ChangeSkyboxMaterial()
    {
        StartCoroutine(ChangeSkyBox());
    }

    public void ChangeRoom()
    {
        StartCoroutine(BackToRoom());
    }

    public void End()
    {
        StartCoroutine(BlackOut());
    }

    IEnumerator ChangeSkyBox()
    {
        FadeOut();
        yield return new WaitForSeconds(duration);
        fadeInSpace?.Invoke();
        RenderSettings.skybox = spaceSkyBox;
        FadeIn();
    }

    IEnumerator BackToRoom()
    {
        FadeOut();
        yield return new WaitForSeconds(duration);
        fadeOutSpace?.Invoke();
        FadeIn();
    }

    IEnumerator BlackOut()
    {
        FadeOut();
        yield return new WaitForSeconds(duration);
    }
}

