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
    private InputActionAsset inputActions;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheckTransform;

    [Space(4)]
    [Header("Animation Variables")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Space(4)]
    [Header("Movement Variables")]
    [SerializeField] private float moveForceWalking;
    [SerializeField] private float moveForceSprinting;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxHorizontalSpeed = 8f;

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

    private const float animSmoothTime = 0.08f;

    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");
    private static readonly int ANIM_IS_GROUNDED = Animator.StringToHash("IsGrounded");
    private static readonly int ANIM_IS_SPRINTING = Animator.StringToHash("IsSprinting");
    private static readonly int ANIM_HORZ = Animator.StringToHash("Horizontal");
    private static readonly int ANIM_VERT = Animator.StringToHash("Vertical");
    private static readonly int ANIM_JUMP_TRIGGER = Animator.StringToHash("JumpTrigger");

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


    private void Awake()
    {
        inputActions    = InputManager.Instance.GetInputActions();
        moveAction      = InputSystem.actions.FindAction("Move");
        jumpAction      = InputSystem.actions.FindAction("Jump");
        sprintAction    = InputSystem.actions.FindAction("Sprint");
    }


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
        UpdateAnimator();

        // Debug.Log("stamina: " + currentStamina + " || regenProgress: " + staminaRegenDelayProgress);
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

        if (animator != null)
        {
            animator.SetTrigger(ANIM_JUMP_TRIGGER);
        }
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

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float physSpeed = 0f;
        if (rb != null)
        {
            physSpeed = Mathf.Abs(rb.linearVelocity.x) / Mathf.Max(0.0001f, maxHorizontalSpeed);
            physSpeed = Mathf.Clamp01(physSpeed);
        }

        animator.SetFloat(ANIM_SPEED, physSpeed, animSmoothTime, Time.deltaTime);
        animator.SetBool(ANIM_IS_GROUNDED, isGrounded);
        animator.SetBool(ANIM_IS_SPRINTING, isSprinting);
        animator.SetFloat(ANIM_HORZ, moveInput.x, animSmoothTime, Time.deltaTime);

        float verticalVel = rb != null ? rb.linearVelocity.y : moveInput.y;
        animator.SetFloat(ANIM_VERT, verticalVel, animSmoothTime, Time.deltaTime);

        if (moveInput.x > 0.01f)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = false;
            else transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput.x < -0.01f)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = true;
            else transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }


    // -------------------------------------------------x-------------------------------------------------
    #endregion
}
