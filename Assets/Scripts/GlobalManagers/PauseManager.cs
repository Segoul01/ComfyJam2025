using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public bool isGamePaused { get; private set; }


    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;

        isGamePaused = false;
    }


    public void PauseGame()
    {
        InputManager.Instance.SwitchActionMap("UI");
        Time.timeScale = 0f;
        isGamePaused = true;
    }


    public void UnPauseGame()
    {
        InputManager.Instance.SwitchActionMap("Player");
        Time.timeScale = 1f;
        isGamePaused = false;
    }
}
