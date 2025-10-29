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
    }


    void OnEnable()
    {
        currentActionMap = DefaultActionMap;
        inputActions.FindActionMap(DefaultActionMap).Enable();
    }


    void OnDisable()
    {
        inputActions.FindActionMap(currentActionMap).Disable();
    }


    public InputActionAsset GetInputActions() => inputActions;


    public void SwitchActionMap(string newActionMap)
    {
        inputActions.FindActionMap(currentActionMap).Disable();
        inputActions.FindActionMap(newActionMap).Enable();
        currentActionMap = newActionMap;
    }
}
