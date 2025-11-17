using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();

    public bool triggerOnce = true;
    private bool triggered = false;

    public bool usePlayerAsDefaultSpeaker = false;

    public UnityEvent onTriggered;

    private Collider2D col2d;

    private void Reset()
    {
        col2d = GetComponent<Collider2D>();
        if (col2d != null) col2d.isTrigger = true;
    }

    private void Awake()
    {
        col2d = GetComponent<Collider2D>();
        if (col2d == null)
            Debug.LogWarning($"{name}: No Collider2D found on DialogueTrigger (RequireComponent should ensure one).");
        else
            col2d.isTrigger = true; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTrigger(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
    }

    private void TryTrigger(GameObject other)
    {
        if (triggerOnce && triggered) return;
        if (other == null) return;
        if (!other.CompareTag("Player")) return;

        if (DialogueManager.Instance != null)
        {
            string defaultSpeaker = usePlayerAsDefaultSpeaker ? DialogueManager.Instance.playerName : null;
            DialogueManager.Instance.StartDialogue(dialogueLines, null, defaultSpeaker);
        }

        onTriggered?.Invoke();

        if (triggerOnce) triggered = true;
    }
}
