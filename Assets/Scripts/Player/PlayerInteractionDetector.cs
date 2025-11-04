using UnityEngine;

public class PlayerInteractionDetector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            PlayerInteractionUI.Instance.ShowText(interactable.GetFullText());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            PlayerInteractionUI.Instance.Hide();
        }
    }
}
