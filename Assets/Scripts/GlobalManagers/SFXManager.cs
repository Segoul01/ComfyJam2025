using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AudioSource audioSource;


    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;

        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }


    public void PlayOneShot(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
}
