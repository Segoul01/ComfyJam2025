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
    public TextMeshProUGUI speakerNameText;
    public Button nextButton;

    [Header("Typewriter")]
    [Tooltip("Delay between characters in seconds (smaller = faster).")]
    public float typingDelay = 0.02f;

    private Queue<string> lines;
    private UnityAction onCompleteCallback;

    // typing control
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentFullLine = "";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        lines = new Queue<string>();

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextLine);

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        if (speakerNameText != null)
            speakerNameText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Returns true if the dialogue UI is currently visible.
    /// </summary>
    public bool IsDialogueActive()
    {
        return dialogPanel != null && dialogPanel.activeSelf;
    }

    /// <summary>
    /// Start a dialogue with a collection of lines.
    /// Optional onComplete and optional speakerName (will display on top).
    /// This overload keeps backwards compatibility.
    /// </summary>
    public void StartDialogue(IEnumerable<string> dialogueLines, UnityAction onComplete = null, string speakerName = null)
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

        if (!string.IsNullOrEmpty(speakerName) && speakerNameText != null)
        {
            speakerNameText.text = speakerName;
            speakerNameText.gameObject.SetActive(true);
        }
        else if (speakerNameText != null)
        {
            speakerNameText.gameObject.SetActive(false);
        }

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        if (InputManager.Instance != null)
            InputManager.Instance.SwitchActionMap("UI");

        ShowNextLine();
    }

    /// <summary>
    /// Convenience: start a single-line dialogue (keeps signature backward compatible).
    /// </summary>
    public void StartSingleLine(string line, UnityAction onComplete = null, string speakerName = null)
    {
        StartDialogue(new[] { line }, onComplete, speakerName);
    }

    /// <summary>
    /// Called by Next button or externally to advance.
    /// If currently typing, this will skip to full line instead of advancing.
    /// </summary>
    public void ShowNextLine()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogText.text = currentFullLine;
            isTyping = false;
            typingCoroutine = null;
            return;
        }

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        string line = lines.Dequeue();
        StartTypingLine(line);
    }

    private void StartTypingLine(string line)
    {
        currentFullLine = line ?? "";
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }
        typingCoroutine = StartCoroutine(TypeLineCoroutine(currentFullLine));
    }

    private IEnumerator TypeLineCoroutine(string line)
    {
        isTyping = true;
        dialogText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            dialogText.text += line[i];
            yield return new WaitForSeconds(typingDelay);
        }

        // finished
        isTyping = false;
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        if (speakerNameText != null)
            speakerNameText.gameObject.SetActive(false);

        if (InputManager.Instance != null)
            InputManager.Instance.SwitchActionMap("Player");

        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
    }
}
