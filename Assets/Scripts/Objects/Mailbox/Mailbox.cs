using UnityEngine;

public class Mailbox : MonoBehaviour
{
    public LetterData letterData;

    public void OnLetterPickedUp()
    {
        Debug.Log("Letter taken from mailbox: " + name);
        // Optional: disable mailbox or visual change  -> TODO think about the mechanic maybe UI can Pop up!
        gameObject.SetActive(false);
    }
}
