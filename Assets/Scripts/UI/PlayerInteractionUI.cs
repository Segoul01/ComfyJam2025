using UnityEngine;
using TMPro;

public class PlayerInteractionUI : MonoBehaviour
{
    public static PlayerInteractionUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI textField;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowText(string text)
    {
        textField.text = text;
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
