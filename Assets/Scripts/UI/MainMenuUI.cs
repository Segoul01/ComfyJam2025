using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private SceneChangerManager GetManager()
    {
        return FindAnyObjectByType<SceneChangerManager>();
    }

    public void OnStart()
    {
        var mgr = GetManager();
        if (mgr != null)
        {
            mgr.LoadNext();
        }
        else
        {
            Debug.LogWarning("SceneChangerManager bulunamad�.");
        }
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
