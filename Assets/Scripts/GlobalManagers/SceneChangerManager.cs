using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class SceneChangerManager : MonoBehaviour
{
    [Header("Scenes (order matters)")]
    [Tooltip("Drag scene names here in the order you want them. Example: MainMenu, GameScene, EndScene")]
    [SerializeField] private List<string> scenes = new List<string>();

    [Space(4)]
    [Header("Startup")]
    [Tooltip("Starting index (0 = first entry in the Scenes list)")]
    [SerializeField] private int startSceneIndex = 0;
    [Tooltip("Keep this manager across scenes? (DontDestroyOnLoad)")]
    [SerializeField] private bool persistAcrossScenes = true;

    [Space(4)]
    [Header("Transition / Fade")]
    [Tooltip("If assigned, fade will be performed on this CanvasGroup during scene changes.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [Tooltip("Fade duration in seconds for fade-out OR fade-in")]
    [SerializeField] private float fadeDuration = 0.25f;
    [Tooltip("Lock manager while a load is in progress (prevent reentrancy)")]
    [SerializeField] private bool lockDuringLoad = true;

    [Space(4)]
    [Header("Events")]
    [Tooltip("Invoked just before a scene change (oldIndex, oldName)")]
    [SerializeField] private SceneChangeEvent OnBeforeSceneChange = new SceneChangeEvent();
    [Tooltip("Invoked right after scene load completes (newIndex, newName)")]
    [SerializeField] private SceneChangeEvent OnAfterSceneLoad = new SceneChangeEvent();

    [System.Serializable]
    public class SceneChangeEvent : UnityEvent<int, string> { }

    private int currentSceneIndex = -1;
    private bool isLoading = false;

    #region Unity lifecycle
    private void Awake()
    {
        if (persistAcrossScenes)
        {
            SceneChangerManager[] existing = FindObjectsOfType<SceneChangerManager>();
            if (existing.Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (scenes == null || scenes.Count == 0)
        {
            Debug.LogWarning("[SceneChangerManager] Scene list is empty. Please add scenes (names must match Build Settings).");
            return;
        }

        startSceneIndex = Mathf.Clamp(startSceneIndex, 0, scenes.Count - 1);

        string activeName = SceneManager.GetActiveScene().name;
        int idx = scenes.IndexOf(activeName);
        if (idx >= 0)
        {
            currentSceneIndex = idx;
            OnAfterSceneLoad?.Invoke(currentSceneIndex, scenes[currentSceneIndex]);
        }
        else
        {
            if (startSceneIndex != currentSceneIndex)
            {
                LoadScene(startSceneIndex);
            }
        }
    }
    #endregion

    #region Public API
    public void LoadScene(int index)
    {
        if (!IsValidIndex(index)) { Debug.LogWarning("[SceneChangerManager] LoadScene: invalid index: " + index); return; }
        if (lockDuringLoad && isLoading) return;
        StartCoroutine(LoadSceneCoroutine(index));
    }

    public void LoadScene(string sceneName)
    {
        int idx = scenes.IndexOf(sceneName);
        if (idx == -1) { Debug.LogWarning("[SceneChangerManager] LoadScene(name): scene not found in list: " + sceneName); return; }
        LoadScene(idx);
    }

    public void LoadNext(bool wrap = false)
    {
        if (scenes.Count == 0) return;
        int next = currentSceneIndex + 1;
        if (next >= scenes.Count)
        {
            if (wrap) next = 0;
            else { Debug.Log("[SceneChangerManager] LoadNext: already last scene."); return; }
        }
        LoadScene(next);
    }

    public void LoadPrevious(bool wrap = false)
    {
        if (scenes.Count == 0) return;
        int prev = currentSceneIndex - 1;
        if (prev < 0)
        {
            if (wrap) prev = scenes.Count - 1;
            else { Debug.Log("[SceneChangerManager] LoadPrevious: already first scene."); return; }
        }
        LoadScene(prev);
    }

    public void ReloadCurrent()
    {
        if (!IsValidIndex(currentSceneIndex)) { Debug.LogWarning("[SceneChangerManager] ReloadCurrent: invalid current index."); return; }
        LoadScene(currentSceneIndex);
    }

    public void LoadByBuildIndex(int buildIndex)
    {
        if (lockDuringLoad && isLoading) return;
        StartCoroutine(LoadBuildIndexCoroutine(buildIndex));
    }
    #endregion

    #region Coroutines
    private IEnumerator LoadSceneCoroutine(int targetIndex)
    {
        isLoading = true;
        int oldIndex = currentSceneIndex;
        string oldName = oldIndex >= 0 && oldIndex < scenes.Count ? scenes[oldIndex] : string.Empty;
        string newName = scenes[targetIndex];

        OnBeforeSceneChange?.Invoke(oldIndex, oldName);

        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(newName);
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }

        currentSceneIndex = targetIndex;
        yield return null;

        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDuration));
        }

        OnAfterSceneLoad?.Invoke(currentSceneIndex, scenes[currentSceneIndex]);
        isLoading = false;
    }

    private IEnumerator LoadBuildIndexCoroutine(int buildIndex)
    {
        isLoading = true;
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDuration));
        }

        string active = SceneManager.GetActiveScene().name;
        int listIdx = scenes.IndexOf(active);
        if (listIdx >= 0) currentSceneIndex = listIdx;

        OnAfterSceneLoad?.Invoke(currentSceneIndex, active);
        isLoading = false;
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float t = 0f;
        fadeCanvasGroup.alpha = from;
        fadeCanvasGroup.blocksRaycasts = true;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float v = Mathf.Lerp(from, to, Mathf.Clamp01(t / Mathf.Max(0.0001f, duration)));
            fadeCanvasGroup.alpha = v;
            yield return null;
        }

        fadeCanvasGroup.alpha = to;
        fadeCanvasGroup.blocksRaycasts = (to > 0.99f);
    }
    #endregion

    #region Helpers
    private bool IsValidIndex(int idx)
    {
        return scenes != null && idx >= 0 && idx < scenes.Count;
    }

    [ContextMenu("Debug: Print Scenes")]
    private void DebugPrintScenes()
    {
        Debug.Log("[SceneChangerManager] scenes count: " + (scenes?.Count ?? 0));
        if (scenes != null)
        {
            for (int i = 0; i < scenes.Count; i++)
                Debug.Log($"[{i}] {scenes[i]}");
        }
    }

    [ContextMenu("Debug: Load Next")]
    private void ContextLoadNext() => LoadNext();

    [ContextMenu("Debug: Load Prev")]
    private void ContextLoadPrev() => LoadPrevious();
    #endregion

    #region Editor helpers (only in the Editor)
#if UNITY_EDITOR
    [ContextMenu("Editor: Fill scenes from Build Settings")]
    private void FillFromBuildSettings()
    {
        scenes.Clear();
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            if (s.enabled)
            {
                string path = s.path;
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                scenes.Add(name);
            }
        }
        Debug.Log("[SceneChangerManager] Filled scenes from Build Settings. Count: " + scenes.Count);
        if (!Application.isPlaying) EditorUtility.SetDirty(this);
    }
#endif
    #endregion
}
