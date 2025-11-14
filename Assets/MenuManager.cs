using UnityEngine;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour
{
    public UnityEvent onRestartGame;

    public void RastartGame()
    {
        PlayerData.Instance.ResetData();
        onRestartGame?.Invoke();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
