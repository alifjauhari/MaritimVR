using Oculus.Platform;
using UnityEngine;
using UnityEngine.Events;

public class ToggleOpenPanel : MonoBehaviour
{
    public bool toggleOpenPanel = false;
    public string openPanelName;
    public string closePanelName;
    public Animator animator;

    public UnityEvent openPanel;
    public UnityEvent closePanel;

    public void OpenClosePanel()
    {
        if (toggleOpenPanel)
        {
            animator.SetTrigger(openPanelName);
            toggleOpenPanel = false;
        }
        else
        {
            animator.SetTrigger(closePanelName);
            toggleOpenPanel = true;
        }
    }

    public void OpenClosePanelEvent()
    {
        if (toggleOpenPanel)
        {
            openPanel?.Invoke();
            toggleOpenPanel = false;
        }
        else
        {
            closePanel?.Invoke();
            toggleOpenPanel = true;
        }
    }
}
