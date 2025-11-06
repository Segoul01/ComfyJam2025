using UnityEngine;
using TMPro;

public class TaskUI : MonoBehaviour
{
    [Tooltip("Matches TaskDefinition.taskID")]
    public string taskID;
    public TMP_Text titleText;
    public TMP_Text progressText;

    private void Start()
    {
        TrySubscribe();
        RefreshInitial();
    }

    private void TrySubscribe()
    {
        if (TaskManager.Instance == null)
        {
            Debug.LogWarning("[TaskUI] TaskManager.Instance is null at Start(). Make sure TaskManager is in scene and Awake runs before UI. Will try again next frame.");
            // Try again next frame
            Invoke(nameof(TrySubscribeDelayed), 0.1f);
            return;
        }
        TaskManager.Instance.OnTaskProgressUpdated.AddListener(OnProgress);
    }

    private void TrySubscribeDelayed()
    {
        if (TaskManager.Instance != null)
            TaskManager.Instance.OnTaskProgressUpdated.AddListener(OnProgress);
        else
            Debug.LogError("[TaskUI] Still cannot find TaskManager.Instance. Ensure a TaskManager GameObject exists in the scene.");
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
            TaskManager.Instance.OnTaskProgressUpdated.RemoveListener(OnProgress);
    }

    private void RefreshInitial()
    {
        if (string.IsNullOrEmpty(taskID)) return;
        if (TaskManager.Instance == null) return;

        var def = TaskManager.Instance.tasks.Find(t => t != null && t.taskID == taskID);
        if (def != null && titleText != null) titleText.text = def.title;
        int cur = TaskManager.Instance.GetProgress(taskID);
        int target = def != null ? Mathf.Max(1, def.targetCount) : 1;
        if (progressText != null) progressText.text = $"{cur}/{target}";
    }

    private void OnProgress(string id, int cur, int target)
    {
        if (id != taskID) return;
        if (progressText != null) progressText.text = $"{cur}/{target}";
    }
}
