using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    public GameObject dialogPanel;   
    public TextMeshProUGUI dialogText;             
    public Button nextButton;          

    private Queue<string> lines;
    private UnityAction onCompleteCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        lines = new Queue<string>();

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextLine);

        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    /// <summary>
    /// Start a dialogue with a collection of lines. Optional callback fires when dialogue ends.
    /// </summary>

    public bool IsDialogueActive()
    {
        return dialogPanel != null && dialogPanel.activeSelf;
    }
    public void StartDialogue(IEnumerable<string> dialogueLines, UnityAction onComplete = null)
    {
        if (dialogueLines == null)
        {
            onComplete?.Invoke();
            return;
        }

        lines.Clear();
        foreach (var l in dialogueLines)
            lines.Enqueue(l);

        onCompleteCallback = onComplete;
        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        var player = FindAnyObjectByType<PlayerMovementManager>();
        if (player != null)
            player.SetMovementLocked(true);

        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        string line = lines.Dequeue();
        if (dialogText != null)
            dialogText.text = line;
    }

    private void EndDialogue()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);


        var player = FindAnyObjectByType<PlayerMovementManager>();
        if (player != null)
            player.SetMovementLocked(false);

        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
    }

    public void StartSingleLine(string line, UnityAction onComplete = null)
    {
        StartDialogue(new[] { line }, onComplete);
    }
}
