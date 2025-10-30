using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public string currentActionMap { get; private set; }
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string DefaultActionMap;


    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
        currentActionMap = DefaultActionMap;
        inputActions.FindActionMap(DefaultActionMap).Enable();
    }


    void OnDisable()
    {
        inputActions.FindActionMap(currentActionMap).Disable();
    }


    public InputActionAsset GetInputActions() => inputActions;


    public void SwitchActionMap(string newActionMap, bool autoEnable = true)
    {
        inputActions.FindActionMap(currentActionMap).Disable();
        if (autoEnable) inputActions.FindActionMap(newActionMap).Enable();
        currentActionMap = newActionMap;
    }


    public void DisableAllInputs()
    {
        inputActions.FindActionMap(currentActionMap).Disable();
    }


    public void EnableAllInputs()
    {
        inputActions.FindActionMap(currentActionMap).Enable();
    }
}
