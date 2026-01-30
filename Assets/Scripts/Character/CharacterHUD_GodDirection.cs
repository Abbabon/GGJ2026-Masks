using UnityEngine;
using UnityEngine.UI;

public class CharacterHUD_GodDirection : MonoBehaviour
{
    [Header("Iris Control")]
    [Tooltip("Controls the fill amount of both Iris_Mask_R and Iris_Mask_L simultaneously (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float irisFillAmount = 0.075f;

    private Transform eyeIconTransform;
    private RectTransform directionControllerTransform;
    private Image irisMaskR;
    private Image irisMaskL;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the EyeIcon child transform
        Transform eyeIcon = transform.Find("EyeIcon");
        if (eyeIcon != null)
        {
            eyeIconTransform = eyeIcon;
        }
        else
        {
            Debug.LogWarning("CharacterHUD_GodDirection: EyeIcon not found as child of DirectionController");
        }

        // Get the RectTransform of this GameObject (DirectionController)
        directionControllerTransform = GetComponent<RectTransform>();

        // Find and cache the Iris Mask Image components
        Transform irisMaskRTransform = transform.Find("Iris_Mask_R");
        if (irisMaskRTransform != null)
        {
            irisMaskR = irisMaskRTransform.GetComponent<Image>();
            if (irisMaskR == null)
            {
                Debug.LogWarning("CharacterHUD_GodDirection: Iris_Mask_R Image component not found");
            }
        }
        else
        {
            Debug.LogWarning("CharacterHUD_GodDirection: Iris_Mask_R not found as child of DirectionController");
        }

        Transform irisMaskLTransform = transform.Find("Iris_Mask_L");
        if (irisMaskLTransform != null)
        {
            irisMaskL = irisMaskLTransform.GetComponent<Image>();
            if (irisMaskL == null)
            {
                Debug.LogWarning("CharacterHUD_GodDirection: Iris_Mask_L Image component not found");
            }
        }
        else
        {
            Debug.LogWarning("CharacterHUD_GodDirection: Iris_Mask_L not found as child of DirectionController");
        }

        // Set initial fill amount
        UpdateIrisFillAmount();
    }

    // Update is called once per frame
    void Update()
    {
        if (eyeIconTransform != null && directionControllerTransform != null)
        {
            // Counter-rotate EyeIcon to always face the same direction
            // Get the current rotation of DirectionController
            float directionControllerZ = directionControllerTransform.localEulerAngles.z;
            
            // Set EyeIcon's rotation to the opposite
            RectTransform eyeIconRect = eyeIconTransform.GetComponent<RectTransform>();
            if (eyeIconRect != null)
            {
                eyeIconRect.localEulerAngles = new Vector3(0, 0, -directionControllerZ);
            }
        }

        // Update iris fill amounts
        UpdateIrisFillAmount();
    }

    /// <summary>
    /// Updates the fill amount of both iris masks to match the irisFillAmount variable
    /// Scales the value so that 1.0 maps to 0.5 for the actual fill amount
    /// </summary>
    private void UpdateIrisFillAmount()
    {
        // Scale the value: when irisFillAmount is 1, apply 0.5 to the masks
        float actualFillAmount = irisFillAmount * 0.5f;

        if (irisMaskR != null)
        {
            irisMaskR.fillAmount = actualFillAmount;
        }

        if (irisMaskL != null)
        {
            irisMaskL.fillAmount = actualFillAmount;
        }
    }

    /// <summary>
    /// Public method to set the iris fill amount programmatically
    /// </summary>
    /// <param name="amount">Fill amount between 0 and 1</param>
    public void SetIrisFillAmount(float amount)
    {
        irisFillAmount = Mathf.Clamp01(amount);
        UpdateIrisFillAmount();
    }

    /// <summary>
    /// Gets the current iris fill amount
    /// </summary>
    /// <returns>Current fill amount (0-1)</returns>
    public float GetIrisFillAmount()
    {
        return irisFillAmount;
    }
}
