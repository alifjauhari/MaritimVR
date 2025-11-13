using UnityEngine;

public class ToggleOpenPanel : MonoBehaviour
{
    public bool toggleOpenPanel = false;
    public string openPanelName;
    public string closePanelName;
    public Animator animator;

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
}
