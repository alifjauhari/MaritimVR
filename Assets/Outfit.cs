using UnityEngine;

public class Outfit : MonoBehaviour
{
    public bool isTrigger = false;
    public Collider outfitCollider;

    private void OnTriggerEnter(Collider other)
    {
        if(other == outfitCollider)
        {
            isTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == outfitCollider)
        {
            isTrigger = false;
        }
    }
}
