using UnityEngine;

public class PlayerParticleManager : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem landImpactParticles;

    private PlayerMovementManager playerMovement;


    void Awake()
    {
        playerMovement = GetComponent<PlayerMovementManager>();
    }


    void Start()
    {
        playerMovement.didJustLand.AddListener(OnPlayerLand);
    }


    void OnPlayerLand()
    {
        landImpactParticles.Play();
    }
}
