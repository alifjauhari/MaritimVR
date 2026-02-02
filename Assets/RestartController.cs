using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RestartController : MonoBehaviour
{

    private bool isHoldingB = false;
    private float holdStartTime = 0f;
    private bool forceEndedByHold = false;
    [SerializeField] private float holdToEndTime = 2f;
    [SerializeField] private UnityEvent onEndDialogue;

    private void Update()
    {
        // Button B (Right Controller - secondary button)
        bool bPressed = UnityEngine.XR.InputDevices
            .GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand)
            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool value) && value;

        if (bPressed && !isHoldingB)
        {
            isHoldingB = true;
            holdStartTime = Time.time;
            forceEndedByHold = false;

            Debug.Log("[VR INPUT] Button B PRESSED - start holding");
        }

        // ---- While Holding
        if (isHoldingB && bPressed)
        {
            float heldTime = Time.time - holdStartTime;
            Debug.Log($"[VR INPUT] Holding B: {heldTime:F2}s");

            // If held long enough → force end immediately
            if (heldTime >= holdToEndTime && !forceEndedByHold)
            {
                forceEndedByHold = true;
                Debug.Log("[VR INPUT] HOLD >= 2s → END DIALOGUE");

                onEndDialogue?.Invoke();   // directly end and trigger onEnd
            }
        }

        // ---- On Release
        if (!bPressed && isHoldingB)
        {
            isHoldingB = false;
        }
    }
}