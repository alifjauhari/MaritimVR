using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public bool isLoungeRoom = false;
    public bool isTugboat = false;

    public static PlayerData Instance;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }


    public void SetTugboat()
    {
        isTugboat = true;
        isLoungeRoom = false;
    }

    public void SetLoungeRoom()
    {
        isLoungeRoom = true;
        isTugboat = false;
    }
}
