using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class OutroController : MonoBehaviour
{
    public PlayableDirector director;            // assign in inspector veya GetComponent ile al
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
        if (director != null) director.stopped += OnDirectorStopped;
    }

    void OnDestroy()
    {
        if (director != null) director.stopped -= OnDirectorStopped;
    }

    void OnDirectorStopped(PlayableDirector d)
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
