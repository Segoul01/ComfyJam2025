using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip menuBGM;
    [SerializeField] private AudioClip inGameBGM;
    

    Task[] task = new Task[1];

    [SerializeField] private float volumeFadeRate = 2f;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;

        if (!audioSource) audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
    }

    private void Start()
    {
        PlayMenuBGM();
    }


    public async void ChangeBGM(AudioClip audioClip)
    {
        if (task[0] != null && !task[0].IsCompleted)
            await task[0];

        if (audioSource.isPlaying)
            await FadeOut();

        audioSource.clip = audioClip;
        audioSource.Play();

        task[0] = FadeIn();
    }


    async Task FadeIn()
    {
        while (audioSource.volume < 1f)
        {
            audioSource.volume += Time.deltaTime * volumeFadeRate;
            await Task.Yield();
        }

        audioSource.volume = 1f;
    }

    async Task FadeOut()
    {
        while (audioSource.volume > 0f)
        {
            try
            {
                audioSource.volume -= Time.deltaTime * volumeFadeRate;
            }
            catch
            {
                audioSource.volume = 0f;
            }
            await Task.Yield();
        }
    }


    public void PlayMenuBGM()
    {
        ChangeBGM(menuBGM);
    }
    

    public void PlayInGameBGM()
    {
        ChangeBGM(inGameBGM);
    }
}
