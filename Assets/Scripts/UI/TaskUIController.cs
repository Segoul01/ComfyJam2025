using UnityEngine;
using TMPro;

public class TaskUIController : MonoBehaviour
{
    [Header("UI refs")]
    public TMP_Text titleText;
    public TMP_Text progressText;
    public GameObject allDonePanel;
    public TMP_Text allDoneText;

    private bool subscribed = false;

    private void Start()
    {
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed) return;

        if (TaskManager.Instance == null)
        {
            Debug.LogWarning("[TaskUIController] TaskManager.Instance is null at Start(). Will retry in 0.2s.");
            Invoke(nameof(TrySubscribe), 0.2f);
            return;
        }

        try
        {
            TaskManager.Instance.OnTaskActivated.AddListener(OnActivated);
            TaskManager.Instance.OnTaskProgressUpdated.AddListener(OnProgress);
            TaskManager.Instance.OnAllTasksCompleted.AddListener(OnAllDone);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[TaskUIController] Subscribe failed: " + ex);
        }

        subscribed = true;
        Debug.Log("[TaskUIController] Subscribed to TaskManager events.");

        var activeDef = TaskManager.Instance.GetActiveTaskDefinition();
        if (activeDef != null)
        {
            OnActivated(activeDef.taskID, activeDef.title, Mathf.Max(1, activeDef.targetCount));

            int cur = TaskManager.Instance.GetProgress(activeDef.taskID);
            OnProgress(activeDef.taskID, cur, Mathf.Max(1, activeDef.targetCount));
        }

    }

    private void OnDestroy()
    {
        if (!subscribed || TaskManager.Instance == null) return;
        TaskManager.Instance.OnTaskActivated.RemoveListener(OnActivated);
        TaskManager.Instance.OnTaskProgressUpdated.RemoveListener(OnProgress);
        TaskManager.Instance.OnAllTasksCompleted.RemoveListener(OnAllDone);
    }

    private void OnActivated(string taskID, string title, int target)
    {
        if (allDonePanel != null) allDonePanel.SetActive(false);
        if (titleText != null) titleText.text = title ?? "";
        if (progressText != null) progressText.text = $"0/{target}";
        Debug.Log($"[TaskUIController] Activated {taskID} : {title} target {target}");
    }

    private void OnProgress(string taskID, int cur, int target)
    {
        if (progressText != null) progressText.text = $"{cur}/{target}";
        Debug.Log($"[TaskUIController] Progress {taskID} {cur}/{target}");
    }

    private void OnAllDone()
    {
        Debug.Log("[TaskUIController] All tasks completed");
        if (allDonePanel != null)
        {
            if (allDoneText != null) allDoneText.text = "All tasks completed";
            allDonePanel.SetActive(true);
        }
        else
        {
            if (titleText != null) titleText.text = "All tasks completed";
            if (progressText != null) progressText.text = "";
        }
    }
}
