using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class NPCInteraction : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea] public string[] startDialogueLines;
    [TextArea] public string[] afterSuccessLines;
    [TextArea] public string[] afterFailLines;
    [TextArea] public string[] alreadyGivenLines;

    [Header("Delivery")]
    public int houseID = -1;

    private bool playerNearby = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (InventoryUI.Instance != null) InventoryUI.Instance.ForceCloseIfForRecipient(houseID);
        }
    }

    private void Update()
    {
        if (!playerNearby) return;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen()) return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive()) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartInteraction();
        }
    }

    private void StartInteraction()
    {
        bool alreadyGiven = false;
        if (LetterDeliverySystem.Instance != null)
        {
            alreadyGiven = LetterDeliverySystem.Instance.IsDelivered(houseID);
        }

        if (alreadyGiven)
        {
            if (DialogueManager.Instance != null && alreadyGivenLines != null && alreadyGivenLines.Length > 0)
                DialogueManager.Instance.StartDialogue(alreadyGivenLines);
            else
                Debug.Log("NPC: already received a letter.");
            return;
        }

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(startDialogueLines, OnDialogueComplete);
        else
            OnDialogueComplete(); // fallback
    }

    private void OnDialogueComplete()
    {
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.ShowForRecipient(houseID, (success, deliveredLetter) =>
            {
                if (success)
                {
                    if (afterSuccessLines != null && afterSuccessLines.Length > 0 && DialogueManager.Instance != null)
                        DialogueManager.Instance.StartDialogue(afterSuccessLines);
                }
                else
                {
                    if (afterFailLines != null && afterFailLines.Length > 0 && DialogueManager.Instance != null)
                        DialogueManager.Instance.StartDialogue(afterFailLines);
                }
            });
        }
        else
        {
            Debug.LogWarning("InventoryUI not present in scene.");
        }
    }
}