using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementManager : MonoBehaviour
{
    #region Variables And Properties

    // --------------------------------------------------------------------------------------------------
    // Input Actions
    // --------------------------------------------------------------------------------------------------

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;


    // --------------------------------------------------------------------------------------------------
    // Inspector variables
    // --------------------------------------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheckTransform;

    [Space(4)]
    [Header("Movement Variables")]
    [SerializeField] private float moveForceWalking;
    [SerializeField] private float moveForceSprinting;
    [SerializeField] private float jumpForce;

    [Space(4)]
    [Header("Configurations")]
    [SerializeField] private float staminaDepletionRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private float staminaRegenDelay;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask whatIsGround;


    // --------------------------------------------------------------------------------------------------
    // Private variables
    // --------------------------------------------------------------------------------------------------

    private Vector2 moveInput;
    private float moveForce;
    private float staminaRegenDelayProgress;


    // --------------------------------------------------------------------------------------------------
    // (Public) Properties
    // --------------------------------------------------------------------------------------------------

    public const float MAXSTAMINA = 100f;
    public float currentStamina { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isSprinting { get; private set; }
    public bool isRegeneratingStamina { get; private set; }


    // -------------------------------------------------x-------------------------------------------------
    #endregion

    #region Methods

    private void Start()
    {
        SetStamina(MAXSTAMINA);
        ResetStaminaRegenDelayProgress();
    }


    private void Update()
    {
        CheckForGrounded();
        CheckForMoveInput();
        CheckForSprintAction();
        CheckForJumpAction();
        TryRegenerateStamina();
    }


    private void CheckForMoveInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        TryMove();
    }


    private void CheckForSprintAction()
    {
        if (sprintAction.IsPressed() && moveInput.x != 0) TrySprint();
        else isSprinting = false;
    }


    private void CheckForJumpAction()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            TryJump();
        }
    }


    public void SetMoveForce(float force)
    {
        moveForce = force;
    }


    public void SetStamina(float value)
    {
        currentStamina = value;
    }


    private void TryMove()
    {
        SetMoveForce(isSprinting ? moveForceSprinting : moveForceWalking);
        rb.AddForce(transform.right * moveInput.x * moveForce * Time.deltaTime * 100f, ForceMode2D.Force);
    }


    private void TrySprint()
    {
        if (currentStamina > 0f && currentStamina - (staminaDepletionRate * Time.deltaTime) >= 0)
        {
            isSprinting = true;
            currentStamina -= staminaDepletionRate * Time.deltaTime;
        }
        else
        {
            currentStamina = 0f;
            isSprinting = false;
        }
    }


    private void TryJump()
    {
        if (!isGrounded)
            return;

        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
    }


    private void ResetStaminaRegenDelayProgress()
    {
        staminaRegenDelayProgress = staminaRegenDelay;
    }


    private void TryRegenerateStamina()
    {
        if (isSprinting)
        {
            ResetStaminaRegenDelayProgress();
            isRegeneratingStamina = false;
            return;
        }

        if (staminaRegenDelayProgress > 0) staminaRegenDelayProgress -= Time.deltaTime;
        else if (currentStamina < MAXSTAMINA) RegenerateStamina();
        else isRegeneratingStamina = false;
    }


    private void RegenerateStamina()
    {
        isRegeneratingStamina = true;
        currentStamina += Time.deltaTime * staminaRegenRate;

        if (currentStamina > MAXSTAMINA) currentStamina = MAXSTAMINA;
    }


    private void CheckForGrounded()
    {
        isGrounded = Physics2D.Raycast(
            groundCheckTransform.position,
            Vector2.down,
            groundCheckRadius,
            whatIsGround
            );
    }


    void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }


    void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    // -------------------------------------------------x-------------------------------------------------
    #endregion
}
