using UnityEngine;

public class BetterGravity : MonoBehaviour
{
    #region Variables

    // --------------------------------------------------------------------------------------------------
    // Inspector variables
    // --------------------------------------------------------------------------------------------------


    [Header("References")]
    [SerializeField] Rigidbody2D rb;


    [Space(4)]


    [Header("Gravity Configs")]
    [SerializeField] float gravityMult = 1.5f;

    // --------------------------------------------------------------------------------------------------
    // Private variables
    // --------------------------------------------------------------------------------------------------


    float origGravity;
    bool isFalling;


    // -------------------------------------------------x-------------------------------------------------
    #endregion

    #region Methods


    void Awake()
    {
        origGravity = rb.gravityScale;
    }


    void Update()
    {
        isFalling = CheckIfFalling();

        rb.gravityScale = isFalling ? origGravity * gravityMult : origGravity;
    }


    bool CheckIfFalling()
    {
        return rb.linearVelocityY < -0.1f ? true : false;
    }

    // -------------------------------------------------x-------------------------------------------------
    #endregion
}