using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[Serializable]
public class DialogueLine
{
    public string speaker; // örn: "Aubrey", "Mr. Twilight" veya "{player}"
    [TextArea(2, 6)]
    public string text;
}

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

    [Header("Player")]
    public string playerName = "Aubrey";

    private Queue<DialogueLine> lines;
    private UnityAction onCompleteCallback;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentFullLine = "";

    private string defaultSpeaker = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        lines = new Queue<DialogueLine>();

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextLine);

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        if (speakerNameText != null)
            speakerNameText.gameObject.SetActive(false);
    }

    public bool IsDialogueActive()
    {
        return dialogPanel != null && dialogPanel.activeSelf;
    }


    public void StartDialogue(IEnumerable<string> dialogueLines, UnityAction onComplete = null, string speakerName = null)
    {
        if (dialogueLines == null)
        {
            onComplete?.Invoke();
            return;
        }

        lines.Clear();

        foreach (var raw in dialogueLines)
        {
            string lineRaw = raw ?? "";

            string parsedSpeaker = null;
            string parsedText = lineRaw;

            int colonIndex = lineRaw.IndexOf(':');
            if (colonIndex > 0)
            {
                string left = lineRaw.Substring(0, colonIndex).Trim();
                string right = lineRaw.Substring(colonIndex + 1).Trim();

                if (!string.IsNullOrEmpty(right))
                {
                    parsedSpeaker = left;
                    parsedText = right;
                }
            }

            if (!string.IsNullOrEmpty(parsedSpeaker) && parsedSpeaker.Contains("{player}"))
                parsedSpeaker = parsedSpeaker.Replace("{player}", playerName);

            var dlgLine = new DialogueLine { speaker = parsedSpeaker, text = parsedText };
            lines.Enqueue(dlgLine);
        }

        onCompleteCallback = onComplete;
        defaultSpeaker = speakerName;

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        if (InputManager.Instance != null)
            InputManager.Instance.SwitchActionMap("UI");

        ShowNextLine();
    }



    public void StartDialogue(IEnumerable<DialogueLine> dialogueLines, UnityAction onComplete = null, string speakerName = null)
    {
        if (dialogueLines == null)
        {
            onComplete?.Invoke();
            return;
        }

        lines.Clear();
        foreach (var l in dialogueLines)
        {
            var copy = new DialogueLine { speaker = l?.speaker, text = l?.text ?? "" };
            if (!string.IsNullOrEmpty(copy.speaker) && copy.speaker.Contains("{player}"))
                copy.speaker = copy.speaker.Replace("{player}", playerName);
            lines.Enqueue(copy);
        }

        onCompleteCallback = onComplete;
        defaultSpeaker = speakerName;

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        if (InputManager.Instance != null)
            InputManager.Instance.SwitchActionMap("UI");

        ShowNextLine();
    }

    public void StartSingleLine(string line, UnityAction onComplete = null, string speakerName = null)
    {
        StartDialogue(new[] { new DialogueLine { speaker = speakerName, text = line } }, onComplete, speakerName);
    }

    public void StartSingleLine(DialogueLine singleLine, UnityAction onComplete = null, string speakerName = null)
    {
        StartDialogue(new[] { singleLine }, onComplete, speakerName);
    }


    public void SetPlayerName(string name)
    {
        playerName = name;
    }


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

        DialogueLine line = lines.Dequeue();
        // satýr bazýnda konuþmacýyý çöz
        string speakerToShow = ResolveSpeaker(line.speaker);
        if (!string.IsNullOrEmpty(speakerToShow) && speakerNameText != null)
        {
            speakerNameText.text = speakerToShow;
            speakerNameText.gameObject.SetActive(true);
        }
        else if (speakerNameText != null)
        {
            speakerNameText.gameObject.SetActive(false);
        }

        StartTypingLine(line.text);
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
        defaultSpeaker = null;
    }

    private string ResolveSpeaker(string lineSpeaker)
    {
        if (!string.IsNullOrEmpty(lineSpeaker))
        {
            return lineSpeaker.Replace("{player}", playerName);
        }

        if (!string.IsNullOrEmpty(defaultSpeaker))
            return defaultSpeaker.Replace("{player}", playerName);

        return null;
    }
}
