using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InfinitySceneController : MonoBehaviour
{
    public GameObject playerController;

    public UnityEvent onBasic;
    public UnityEvent onLoungeRoom;
    public UnityEvent onTugBoat;

    private void Start()
    {
        if (PlayerData.Instance.isLoungeRoom)
        {
            onLoungeRoom.Invoke();
        }
        else if (PlayerData.Instance.isTugboat)
        {
            onTugBoat.Invoke();
        }
        else
        {
            onBasic.Invoke();
        }
    }

    public void SetPlayerLocation(Transform parent)
    {
        playerController.transform.SetParent(parent);
        playerController.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        StartCoroutine(WaitAndSetPlayerLocation());
    }

    IEnumerator WaitAndSetPlayerLocation()
    {
        yield return new WaitForSeconds(0.1f);
        playerController.transform.SetParent(null);
    }

    public void SetPlayerPosition(Transform positionTarget)
    {
        playerController.transform.SetPositionAndRotation(positionTarget.position, positionTarget.rotation);
    }
}
