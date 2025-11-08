using Oculus.VoiceSDK.UX;
using UnityEngine;
using UnityEngine.Events;

public class OutfitManager : MonoBehaviour
{
    [Header("Bottom")]
    [SerializeField] SkinnedMeshRenderer bottomBasic;
    [SerializeField] SkinnedMeshRenderer bottomWorker;
    [SerializeField] Material bottomMaterial;
    [SerializeField] Outfit bottomOutfit;
    [SerializeField] UnityEvent onBottomUsed;

    [Header("Top"), Space(10)]
    [SerializeField] SkinnedMeshRenderer topBasic;
    [SerializeField] SkinnedMeshRenderer topWorker;
    [SerializeField] Material topMaterial;
    [SerializeField] Outfit topOutfit;
    [SerializeField] UnityEvent onTopUsed;

    [Header("Shoes"), Space(10)]
    [SerializeField] SkinnedMeshRenderer shoesBasic;
    [SerializeField] SkinnedMeshRenderer shoesWorker;
    [SerializeField] Material shoesMaterial;
    [SerializeField] Outfit shoesOutfit;
    [SerializeField] UnityEvent onShoesUsed;

    [Header("EyeWear"), Space(10)]
    [SerializeField] SkinnedMeshRenderer eyewearBasic;
    [SerializeField] SkinnedMeshRenderer eyewearWorker;
    [SerializeField] Material eyewearMaterial;
    [SerializeField] Outfit eyewearOutfit;
    [SerializeField] UnityEvent onEyewearUsed;

    [Header("Hat"), Space(10)]
    [SerializeField] SkinnedMeshRenderer hatBasic;
    [SerializeField] SkinnedMeshRenderer hatWorker;
    [SerializeField] Material hatMaterial;
    [SerializeField] Outfit hatOutfit;
    [SerializeField] UnityEvent onHatUsed;

    [Header("Outfit Complete"), Space(10)]
    public UnityEvent onOutfitComplete;
    public UnityEvent onTopAndBottom;

    public void SelectBottom()
    {
        bottomBasic.enabled = false;
        bottomWorker.enabled = true;
    }

    public void UnselectBottom()
    {
        if (bottomOutfit.isTrigger)
        {
            bottomBasic.enabled = false;
            bottomWorker.material = bottomMaterial;
            bottomOutfit.isUsed = true;
            onBottomUsed?.Invoke();
            bottomOutfit.gameObject.SetActive(false);
        }
        else
        {
            bottomBasic.enabled = true;
            bottomWorker.enabled = false;
        }
    }

    public void SelectTop()
    {
        topBasic.enabled = false;
        topWorker.enabled = true;
    }

    public void UnselectTop()
    {
        if (topOutfit.isTrigger)
        {
            topBasic.enabled= false;
            topWorker.material = topMaterial;
            topOutfit.gameObject.SetActive(false);
            topOutfit.isUsed = true;
            onTopUsed?.Invoke();
        }
        else
        {
            topBasic.enabled = true;
            topWorker.enabled = false;
        }
    }

    public void SelectShoes()
    {
        shoesBasic.enabled = false;
        shoesWorker.enabled = true;
    }

    public void UnselectShoes()
    {
        if (shoesOutfit.isTrigger)
        {
            shoesBasic.enabled = false;
            shoesWorker.material = shoesMaterial;
            shoesOutfit.gameObject.SetActive(false);
            shoesOutfit.isUsed = true;
            onShoesUsed?.Invoke();
        }
        else
        {
            shoesBasic.enabled = true;
            shoesWorker.enabled = false;
        }
    }

    public void SelectEyewear()
    {
        eyewearBasic.enabled = false;
        eyewearWorker.enabled = true;
    }

    public void UnselectEyewear()
    {
        if (eyewearOutfit.isTrigger)
        {
            eyewearBasic.enabled = false;
            eyewearWorker.material = eyewearMaterial;
            eyewearOutfit.gameObject.SetActive(false);
            eyewearOutfit.isUsed = true;
            onEyewearUsed?.Invoke();
        }
        else
        {
            eyewearBasic.enabled = true;
            eyewearWorker.enabled = false;
        }
    }

    public void SelectHat()
    {
        hatBasic.enabled = false;
        hatWorker.enabled = true;
    }

    public void UnselectHat()
    {
        if (hatOutfit.isTrigger)
        {
            hatBasic.enabled = false;
            hatWorker.material = hatMaterial;
            hatOutfit.gameObject.SetActive(false);
            hatOutfit.isUsed = true;
            onHatUsed?.Invoke();
        }
        else
        {
            hatBasic.enabled = true;
            hatWorker.enabled = false;
        }
    }

    public void CheckOutfit()
    {
        if(bottomOutfit.isUsed && topOutfit.isUsed && shoesOutfit.isUsed && eyewearOutfit.isUsed && hatOutfit.isUsed)
        {
            onOutfitComplete?.Invoke();
        }
    }

    public void TopsAndBottom()
    {
        if (bottomOutfit.isUsed && topOutfit.isUsed)
        {
            onTopAndBottom?.Invoke();
        }
    }
}
