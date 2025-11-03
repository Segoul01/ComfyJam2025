using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string displayName = "Object";
    public string interactionText = "Interact";

    [Tooltip("Optional: custom tag to check logic if needed.")]
    public string typeTag; 

    public string GetFullText()
    {
        return $"{interactionText} {displayName}!";
    }
}
