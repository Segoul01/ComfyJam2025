using UnityEngine;

public class CutsceneInputManager : MonoBehaviour
{
    public void EnableInput()
    {
        InputManager.Instance.EnableAllInputs();
    }
    

    public void DisableInput()
    {
        InputManager.Instance.DisableAllInputs();
    }
}
