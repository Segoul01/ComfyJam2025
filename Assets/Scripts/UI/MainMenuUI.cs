using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private SceneChangerManager GetManager()
    {
        return FindObjectOfType<SceneChangerManager>();
    }

    public void OnStart()
    {
        var mgr = GetManager();
        if (mgr != null)
        {
            mgr.LoadScene("IntroCut"); 
        }
        else
        {
            Debug.LogWarning("SceneChangerManager bulunamadý.");
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
