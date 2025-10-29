using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }
    public bool IsGamePaused { get => PauseManager.Instance.isGamePaused; }

    [SerializeField] private GameObject pauseMenu;
    private InputAction menuOpenAction;
    private InputAction menuCloseAction;


    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;

        menuOpenAction  = InputSystem.actions.FindAction("MenuOpen");
        menuCloseAction = InputSystem.actions.FindAction("MenuClose");
    }


    private void Update()
    {
        GetInput();
    }


    private void GetInput()
    {
        if (IsGamePaused)
        {
            if (menuCloseAction.WasCompletedThisFrame())
                HidePauseMenu();
        }
        else
        {
            if (menuOpenAction.WasCompletedThisFrame())
                ShowPauseMenu();
        }
    }


    // Show the Pause Menu
    private void ShowPauseMenu()
    {
        PauseManager.Instance.PauseGame();
        pauseMenu?.SetActive(true);
    }
    
    
    // Remove the Pause Menu
    private void HidePauseMenu()
    {
        PauseManager.Instance.UnPauseGame();
        pauseMenu?.SetActive(false);
    }
    
}
