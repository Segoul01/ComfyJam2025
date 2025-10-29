using UnityEngine;
using UnityEngine.InputSystem;

public class LetterPickupSystem : MonoBehaviour
{
    public static LetterPickupSystem Instance { get; private set; }

    private PlayerInventoryManager inventory;
    private Mailbox currentMailbox;
    private InputAction interactAction;

    private void Awake()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        inventory = GetComponent<PlayerInventoryManager>();
        if (inventory == null)
            Debug.LogError("PlayerInventoryManager not found on player!");
    }

    private void Update()
    {
        if (currentMailbox != null && interactAction.WasPressedThisFrame()) PickupLetter();

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Mailbox mailbox = other.GetComponent<Mailbox>();
        if (mailbox != null)
        {
            currentMailbox = mailbox;
            Debug.Log("Mailbox nearby: " + mailbox.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Mailbox mailbox = other.GetComponent<Mailbox>();
        if (mailbox != null && mailbox == currentMailbox)
        {
            currentMailbox = null;
            Debug.Log("Left mailbox area");
        }
    }

    private void PickupLetter()
    {
        if (currentMailbox == null || currentMailbox.letterData == null)
            return;

        inventory.AddLetter(currentMailbox.letterData);
        Debug.Log("Letter picked up: " + currentMailbox.letterData.title);

        currentMailbox.OnLetterPickedUp();
        currentMailbox = null;
    }
}
