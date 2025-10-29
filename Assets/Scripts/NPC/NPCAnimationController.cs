using UnityEngine;

public class NPCAnimationController : MonoBehaviour
{
    #region Variables And Properties

    // --------------------------------------------------------------------------------------------------
    // Inspector variables
    // --------------------------------------------------------------------------------------------------

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // --------------------------------------------------------------------------------------------------
    // Private variables
    // --------------------------------------------------------------------------------------------------

    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");
    private static readonly int ANIM_IS_GROUNDED = Animator.StringToHash("IsGrounded");
    private static readonly int ANIM_IS_SPRINTING = Animator.StringToHash("IsSprinting");
    private static readonly int ANIM_HORZ = Animator.StringToHash("Horizontal");
    private static readonly int ANIM_VERT = Animator.StringToHash("Vertical");

    private const float animSmoothTime = 0.08f;

    // -------------------------------------------------x-------------------------------------------------
    #endregion

    #region Methods
    public void ApplyMovement(Vector2 velocity, bool grounded, bool sprinting)
    {
        if (animator == null) return;

        float speed = Mathf.Abs(velocity.x);

        animator.SetFloat(ANIM_SPEED, speed, animSmoothTime, Time.deltaTime);
        animator.SetBool(ANIM_IS_GROUNDED, grounded);
        animator.SetBool(ANIM_IS_SPRINTING, sprinting);
        animator.SetFloat(ANIM_HORZ, velocity.x, animSmoothTime, Time.deltaTime);
        animator.SetFloat(ANIM_VERT, velocity.y, animSmoothTime, Time.deltaTime);

        if (velocity.x > 0.01f) spriteRenderer.flipX = false;
        else if (velocity.x < -0.01f) spriteRenderer.flipX = true;
    }
    #endregion
}
