using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSfx : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }


    private void OnButtonClick()
    {
        SFXManager.Instance.PlayUIClick();
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(OnButtonClick);
    }
}
