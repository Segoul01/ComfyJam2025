using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AudioSource audioSource;


    [Header("UI")]
    [SerializeField] private AudioClip[] clicks;


    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);

        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }


    public void PlayOneShot(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }


    public void PlayUIClick()
    {
        PlayOneShot(clicks[Random.Range(0, clicks.Length-1)]);
    }
}
