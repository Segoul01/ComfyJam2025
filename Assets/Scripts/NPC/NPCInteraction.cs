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
    [TextArea] public string[] afterTalkLines;

    [Header("Delivery")]
    public int houseID = -1;
    [Tooltip("If false, this NPC will NOT accept letters and inventory won't open.")]
    public bool acceptsLetters = true;

    private bool hasTalkedOnce = false;
    private bool playerNearby = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (InventoryUI.Instance != null)
                InventoryUI.Instance.ForceCloseIfForRecipient(houseID);
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
        if (acceptsLetters)
        {
            bool alreadyGiven = false;
            if (LetterDeliverySystem.Instance != null)
                alreadyGiven = LetterDeliverySystem.Instance.IsDelivered(houseID);

            if (alreadyGiven)
            {
                if (DialogueManager.Instance != null && alreadyGivenLines != null && alreadyGivenLines.Length > 0)
                    DialogueManager.Instance.StartDialogue(alreadyGivenLines, null, gameObject.name);
                else
                    Debug.Log("NPC: already received a letter.");
                return;
            }
        }

        if (acceptsLetters)
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(startDialogueLines, OnDialogueComplete, gameObject.name);
            else
                OnDialogueComplete();
        }
        else
        {
            if (!hasTalkedOnce)
            {
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(startDialogueLines, () =>
                    {
                        hasTalkedOnce = true;

                        if (TaskManager.Instance != null)
                            TaskManager.Instance.NotifyTalkedTo(gameObject.name, houseID);
                    }, gameObject.name);
                }
                else
                {
                    hasTalkedOnce = true;
                    if (TaskManager.Instance != null)
                        TaskManager.Instance.NotifyTalkedTo(gameObject.name, houseID);
                }
            }
            else
            {
                if (DialogueManager.Instance != null && afterTalkLines != null && afterTalkLines.Length > 0)
                {
                    DialogueManager.Instance.StartDialogue(afterTalkLines, () =>
                    {
                        if (TaskManager.Instance != null)
                            TaskManager.Instance.NotifyTalkedTo(gameObject.name, houseID);
                    }, gameObject.name);
                }
            }
        }
    }

    private void OnDialogueComplete()
    {
        if (acceptsLetters)
        {
            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.ShowForRecipient(houseID, (success, deliveredLetter) =>
                {
                    if (success)
                    {
                        if (afterSuccessLines != null && afterSuccessLines.Length > 0 && DialogueManager.Instance != null)
                            DialogueManager.Instance.StartDialogue(afterSuccessLines, null, gameObject.name);

                        if (TaskManager.Instance != null)
                            TaskManager.Instance.NotifyLetterDelivered(deliveredLetter, houseID);
                    }
                    else
                    {
                        if (afterFailLines != null && afterFailLines.Length > 0 && DialogueManager.Instance != null)
                            DialogueManager.Instance.StartDialogue(afterFailLines, null, gameObject.name);
                    }
                });
            }
            else
            {
                Debug.LogWarning("InventoryUI not present in scene.");
            }
        }
    }
}
