using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Panels")]
    public GameObject inventoryPanel;
    public Transform listParent;
    public GameObject letterButtonPrefab;
    public GameObject letterFullPanel;
    public TMP_Text fullTitleText;
    public TMP_Text fullContentText;
    public Button giveButton;
    public Button cancelButton;
    public Button closeButton;

    [Header("Scroll")]
    public ScrollRect letterScrollRect;
    public RectTransform listParentRect;

    [Header("Options")]
    public bool allowToggleWithI = true;

    private PlayerInventoryManager playerInventory;
    private int currentRecipientHouseID = -1;
    private LetterData selectedLetter;
    private Action<bool, LetterData> onDeliveryResultCallback;
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private bool isOpenForDelivery = false;
    private bool openedAsGeneralInventory = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (letterFullPanel != null) letterFullPanel.SetActive(false);
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerInventory = player.GetComponent<PlayerInventoryManager>();
        if (playerInventory == null) Debug.LogError("InventoryUI: PlayerInventoryManager not found on Player (Player tag required).");

        if (giveButton != null) giveButton.onClick.AddListener(OnGiveClicked);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClicked);
        if (closeButton != null) closeButton.onClick.AddListener(OnCancelClicked);
    }

    private void Update()
    {
        if (allowToggleWithI && Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                CloseInventory();
            }
            else
            {
                ShowForRecipient(-1, null, openedAsGeneralInventory: true);
            }
        }

        if (inventoryPanel != null && inventoryPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseInventory();
        }
    }

    /// <summary>
    /// Show inventory for a recipient. If openedAsGeneralInventory == true -> no direct-deliver behavior.
    /// </summary>
    public void ShowForRecipient(int recipientHouseID, Action<bool, LetterData> resultCallback, bool openedAsGeneralInventory = false)
    {
        if (playerInventory == null) { Debug.LogWarning("InventoryUI: no player inventory."); resultCallback?.Invoke(false, null); return; }

        currentRecipientHouseID = recipientHouseID;
        onDeliveryResultCallback = resultCallback;
        isOpenForDelivery = !openedAsGeneralInventory && resultCallback != null;
        this.openedAsGeneralInventory = openedAsGeneralInventory;

        if (isOpenForDelivery)
        {
            // var player = FindAnyObjectByType<PlayerMovementManager>();
            // if (player != null)
            //     player.SetMovementLocked(true);
            InputManager.Instance.SwitchActionMap("UI");
        }

        PopulateList();
        letterFullPanel?.SetActive(false);
        inventoryPanel?.SetActive(true);

        if (spawnedButtons.Count > 0)
        {
            var firstBtn = spawnedButtons[0].GetComponentInChildren<Button>();
            if (firstBtn != null) EventSystem.current.SetSelectedGameObject(firstBtn.gameObject);
        }
    }


    private void PopulateList()
    {
        ClearList();

        var letters = playerInventory.GetLetters();
        if (letters == null || letters.Count == 0) return;

        for (int i = 0; i < letters.Count; i++)
        {
            LetterData l = letters[i];

            var go = Instantiate(letterButtonPrefab);
            go.transform.SetParent(listParent, false); // <-- preserves prefab's local position/anchors/pivot
            spawnedButtons.Add(go);

            var btn = go.GetComponent<Button>() ?? go.GetComponentInChildren<Button>();

            TMP_Text senderTmp = null;
            TMP_Text addressTmp = null;

            var senderTf = go.transform.Find("Sender");
            if (senderTf != null) senderTmp = senderTf.GetComponent<TMP_Text>();

            var addressTf = go.transform.Find("Address");
            if (addressTf != null) addressTmp = addressTf.GetComponent<TMP_Text>();

            string maskedSender = string.IsNullOrEmpty(l.sender) ? "[Unknown]" : MaskString(l.sender);
            string maskedAddress = string.IsNullOrEmpty(l.address) ? "[Unknown]" : MaskString(l.address);

            if (senderTmp != null) senderTmp.text = $"FROM: {maskedSender}";
            else Debug.LogWarning($"InventoryUI: prefab '{letterButtonPrefab.name}' missing child 'Sender' (TMP_Text).");

            if (addressTmp != null) addressTmp.text = $"ADDRESS: {maskedAddress}";
            else Debug.LogWarning($"InventoryUI: prefab '{letterButtonPrefab.name}' missing child 'Address' (TMP_Text).");

            var allTmps = go.GetComponentsInChildren<TMP_Text>();
            foreach (var t in allTmps)
            {
                if (t == senderTmp || t == addressTmp) continue;
                t.text = string.Empty;
            }

            var allLegacy = go.GetComponentsInChildren<Text>();
            foreach (var t in allLegacy) t.text = string.Empty;

            // Assign icon if you still want it (optional)
            Image iconImg = null;
            var iconTf = go.transform.Find("Icon");
            if (iconTf != null) iconImg = iconTf.GetComponent<Image>();
            if (iconImg == null)
            {
                var imgs = go.GetComponentsInChildren<Image>();
                foreach (var im in imgs)
                {
                    if (im.gameObject == go) continue;
                    iconImg = im;
                    break;
                }
            }
            if (iconImg != null && l.icon != null) iconImg.sprite = l.icon;

            int idx = i;
            if (btn != null)
            {
                if (isOpenForDelivery)
                {
                    var capturedLetter = l;
                    btn.onClick.AddListener(() => AttemptDeliverFromList(capturedLetter));
                }
                else
                {
                    btn.onClick.AddListener(() => OnLetterSelected(idx));
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        RectTransform contentRect = listParent as RectTransform;
        if (contentRect != null)
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        if (letterScrollRect != null)
            letterScrollRect.verticalNormalizedPosition = 1f;
    }

    private void ClearList()
    {
        foreach (var g in spawnedButtons)
        {
            if (g == null) continue;

            var btn = g.GetComponent<Button>() ?? g.GetComponentInChildren<Button>();
            if (btn != null) btn.onClick.RemoveAllListeners();

            var imgs = g.GetComponentsInChildren<Image>();
            foreach (var im in imgs) if (im != null) im.sprite = null;

            var tmps = g.GetComponentsInChildren<TMP_Text>();
            foreach (var t in tmps) t.text = string.Empty;
            var legacy = g.GetComponentsInChildren<Text>();
            foreach (var t in legacy) t.text = string.Empty;

            Destroy(g);
        }
        spawnedButtons.Clear();

        StartCoroutine(UnloadUnusedAssetsCoroutine());
    }
    private void OnLetterSelected(int index)
    {
        var letters = playerInventory.GetLetters();
        if (index < 0 || index >= letters.Count) return;
        selectedLetter = letters[index];
       // ShowFullLetter(selectedLetter);
    }

    private void ShowFullLetter(LetterData letter)
    {
        if (letterFullPanel != null)
        {
            letterFullPanel.SetActive(true);
            if (fullTitleText != null) fullTitleText.text = letter.title;
            else
            {
                var t1 = letterFullPanel.GetComponentInChildren<TMP_Text>();
                if (t1 != null) t1.text = letter.title;
                else
                {
                    var t2 = letterFullPanel.GetComponentInChildren<Text>();
                    if (t2 != null) t2.text = letter.title;
                }
            }

            // We are not revealing the body — show masked sender/receiver/address
            string maskedSender = MaskString(letter.sender);
            string maskedReceiver = MaskString(letter.receiver);
            string maskedAddress = string.IsNullOrEmpty(letter.address) ? "" : MaskString(letter.address);

            string display = $"From: {maskedSender}\nTo:   {maskedReceiver}";
            if (!string.IsNullOrEmpty(maskedAddress))
                display += $"\nAddr: {maskedAddress}";

            display += "\n\n[Content hidden]";

            if (fullContentText != null) fullContentText.text = display;
            else
            {
                var c1 = letterFullPanel.transform.Find("Content")?.GetComponent<TMP_Text>();
                if (c1 != null) c1.text = display;
                else
                {
                    var c2 = letterFullPanel.GetComponentInChildren<Text>();
                    if (c2 != null) c2.text = display;
                }
            }

            if (openedAsGeneralInventory)
            {
                if (giveButton != null) giveButton.gameObject.SetActive(false);

                if (cancelButton != null)
                {
                    TMP_Text tmp = cancelButton.GetComponentInChildren<TMP_Text>();
                    if (tmp != null) tmp.text = "Close";
                    else
                    {
                        var legacy = cancelButton.GetComponentInChildren<Text>();
                        if (legacy != null) legacy.text = "Close";
                    }
                }
            }
            else
            {
                if (giveButton != null) giveButton.gameObject.SetActive(true);

                if (cancelButton != null)
                {
                    TMP_Text tmp = cancelButton.GetComponentInChildren<TMP_Text>();
                    if (tmp != null) tmp.text = "Cancel";
                    else
                    {
                        var legacy = cancelButton.GetComponentInChildren<Text>();
                        if (legacy != null) legacy.text = "Cancel";
                    }
                }
            }
        }
    }

    private void OnGiveClicked()
    {
        if (selectedLetter == null)
        {
            Debug.Log("No letter selected.");
            return;
        }

        bool success = false;
        if (LetterDeliverySystem.Instance != null)
        {
            success = LetterDeliverySystem.Instance.AttemptDeliver(selectedLetter, currentRecipientHouseID);
        }
        else
        {
            success = (selectedLetter.houseID == currentRecipientHouseID);
        }

        if (success)
        {
            playerInventory.RemoveLetter(selectedLetter);
            Debug.Log("Delivery success: " + selectedLetter.title);
        }
        else
        {
            Debug.Log("Delivery failed: " + selectedLetter.title);
        }

        // IMPORTANT: close inventory before invoking callback so dialog/UI won't overlap and break.
        var cb = onDeliveryResultCallback;
        CloseInventory();
        cb?.Invoke(success, selectedLetter);
    }

    private void OnCancelClicked()
    {
        selectedLetter = null;
        if (letterFullPanel != null) letterFullPanel.SetActive(false);

        if (openedAsGeneralInventory && giveButton != null)
            giveButton.gameObject.SetActive(false);
    }

    public void CloseInventory()
    {
        ClearList();
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (letterFullPanel != null) letterFullPanel.SetActive(false);
        selectedLetter = null;
        currentRecipientHouseID = -1;
        // keep callback reference until caller has been invoked (we clear it in callers)
        isOpenForDelivery = false;
        openedAsGeneralInventory = false;

        // var player = FindAnyObjectByType<PlayerMovementManager>();
        // if (player != null)
        //     player.SetMovementLocked(false);
        InputManager.Instance.SwitchActionMap("Player");
    }

    public void ForceCloseIfForRecipient(int houseID)
    {
        if (isOpenForDelivery && currentRecipientHouseID == houseID)
        {
            CloseInventory();
        }
    }

    public bool IsOpen()
    {
        return inventoryPanel != null && inventoryPanel.activeSelf;
    }

    // ---------- NEW/HELPER METHODS ----------

    private void AttemptDeliverFromList(LetterData letter)
    {
        if (letter == null) return;

        bool success = false;
        if (LetterDeliverySystem.Instance != null)
            success = LetterDeliverySystem.Instance.AttemptDeliver(letter, currentRecipientHouseID);
        else
            success = (letter.houseID == currentRecipientHouseID);

        if (success)
        {
            playerInventory.RemoveLetter(letter);
            Debug.Log("Delivery success (from list): " + letter.title);
            var cb = onDeliveryResultCallback;
            CloseInventory();
            cb?.Invoke(true, letter);
        }
        else
        {
            Debug.Log("Delivery failed (from list): " + letter.title);
            // Close inventory first so dialog system isn't overlapping UI
            var cb = onDeliveryResultCallback;
            CloseInventory();
            cb?.Invoke(false, letter);
            // keep inventory closed to avoid dialog overlap (fixes the bug you reported)
        }
    }

    private string MaskString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            string w = words[i];
            if (w.Length <= 2)
            {
                words[i] = "__";
            }
            else
            {
                words[i] = $"{w[0]}__{w[w.Length - 1]}";
            }
        }
        return string.Join(" ", words);
    }

    private IEnumerator UnloadUnusedAssetsCoroutine()
    {
        yield return Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}
