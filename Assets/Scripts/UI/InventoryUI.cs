using System;
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
    /// Show inventory for a recipient. If openedAsGeneralInventory == true -> no Give button.
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
            var player = FindAnyObjectByType<PlayerMovementManager>();
            if (player != null)
                player.SetMovementLocked(true);
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
            var go = Instantiate(letterButtonPrefab, listParent);
            spawnedButtons.Add(go);

            var btn = go.GetComponent<Button>() ?? go.GetComponentInChildren<Button>();

            TMP_Text titleTmp = go.GetComponentInChildren<TMP_Text>();
            if (titleTmp == null)
            {
                var t = go.transform.Find("Title");
                if (t != null) titleTmp = t.GetComponent<TMP_Text>();
            }
            if (titleTmp != null) titleTmp.text = l.title;
            else
            {
                var legacy = go.GetComponentInChildren<Text>();
                if (legacy != null) legacy.text = l.title;
            }

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
            if (btn != null) btn.onClick.AddListener(() => OnLetterSelected(idx));
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
        foreach (var g in spawnedButtons) Destroy(g);
        spawnedButtons.Clear();
    }

    private void OnLetterSelected(int index)
    {
        var letters = playerInventory.GetLetters();
        if (index < 0 || index >= letters.Count) return;
        selectedLetter = letters[index];
        ShowFullLetter(selectedLetter);
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

            if (fullContentText != null) fullContentText.text = $"From: {letter.sender}\nTo: {letter.receiver}\n\n{letter.content}";
            else
            {
                var c1 = letterFullPanel.transform.Find("Content")?.GetComponent<TMP_Text>();
                if (c1 != null) c1.text = $"From: {letter.sender}\nTo: {letter.receiver}\n\n{letter.content}";
                else
                {
                    var c2 = letterFullPanel.GetComponentInChildren<Text>();
                    if (c2 != null) c2.text = $"From: {letter.sender}\nTo: {letter.receiver}\n\n{letter.content}";
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

        onDeliveryResultCallback?.Invoke(success, selectedLetter);
        CloseInventory();
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
        onDeliveryResultCallback = null;
        isOpenForDelivery = false;
        openedAsGeneralInventory = false;

        var player = FindAnyObjectByType<PlayerMovementManager>();
        if (player != null)
            player.SetMovementLocked(false);
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
}
