using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable] public class TaskProgressUnityEvent : UnityEvent<string, int, int> { }
[System.Serializable] public class TaskCompletedUnityEvent : UnityEvent<string> { }
[System.Serializable] public class TaskActivatedUnityEvent : UnityEvent<string, string, int> { }

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("Task Definitions (drag ScriptableObjects here)")]
    public List<TaskDefinition> tasks = new List<TaskDefinition>();
    [Header("Scene bindings (assign scene objects here, not in ScriptableObjects)")]
    public List<TaskSceneBinding> sceneBindings = new List<TaskSceneBinding>();


    [Header("Mode")]
    public bool sequentialMode = true;
    public int startIndex = 0; 

    private Dictionary<string, int> progress = new Dictionary<string, int>();
    private HashSet<string> completed = new HashSet<string>();
    private int activeTaskIndex = -1;

    [Header("Events")]
    public TaskActivatedUnityEvent OnTaskActivated;
    public TaskProgressUnityEvent OnTaskProgressUpdated;
    public TaskCompletedUnityEvent OnTaskCompleted;
    public UnityEvent OnAllTasksCompleted; 

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // --- ADD THESE LINES: ensure UnityEvent fields are initialized ---
        if (OnTaskActivated == null) OnTaskActivated = new TaskActivatedUnityEvent();
        if (OnTaskProgressUpdated == null) OnTaskProgressUpdated = new TaskProgressUnityEvent();
        if (OnTaskCompleted == null) OnTaskCompleted = new TaskCompletedUnityEvent();
        if (OnAllTasksCompleted == null) OnAllTasksCompleted = new UnityEvent();
        // -------------------------------------------------------------------

        progress.Clear();
        completed.Clear();
        for (int i = 0; i < tasks.Count; i++)
        {
            var t = tasks[i];
            if (t == null) continue;
            if (string.IsNullOrEmpty(t.taskID))
            {
                Debug.LogWarning($"TaskManager: TaskDefinition '{t.name}' has empty taskID. Please set it.");
                continue;
            }
            if (!progress.ContainsKey(t.taskID)) progress[t.taskID] = 0;
        }

        if (sequentialMode)
        {
            activeTaskIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, tasks.Count - 1));
            ActivateTaskAtIndex(activeTaskIndex);
        }
        else
        {
            foreach (var t in tasks)
                if (t != null) OnTaskActivated?.Invoke(t.taskID, t.title, Mathf.Max(1, t.targetCount));
        }
    }

    public TaskDefinition GetActiveTaskDefinition()
    {
        if (sequentialMode)
        {
            if (activeTaskIndex >= 0 && activeTaskIndex < tasks.Count)
                return tasks[activeTaskIndex];
            return null;
        }
        else
        {
            foreach (var t in tasks)
                if (t != null && !IsCompleted(t.taskID))
                    return t;
            return tasks.Count > 0 ? tasks[0] : null;
        }
    }

    public void IncrementTask(string taskID, int amount = 1)
    {
        if (string.IsNullOrEmpty(taskID)) return;
        if (!progress.ContainsKey(taskID)) progress[taskID] = 0;
        if (completed.Contains(taskID)) return;

        progress[taskID] += amount;
        int cur = progress[taskID];

        var def = tasks.Find(x => x != null && x.taskID == taskID);
        int target = def != null ? Mathf.Max(1, def.targetCount) : 1;

        Debug.Log($"[TaskManager] Progress: {taskID} -> {cur}/{target}");
        OnTaskProgressUpdated?.Invoke(taskID, cur, target);

        if (cur >= target)
            CompleteTask(taskID);
    }

    private void CompleteTask(string taskID)
    {
        if (completed.Contains(taskID)) return;
        completed.Add(taskID);

        var def = tasks.Find(x => x != null && x.taskID == taskID);
        if (def != null)
        {
            ExecuteCompletionActions(def);
        }

        OnTaskCompleted?.Invoke(taskID);
        Debug.Log($"[TaskManager] Task completed: {taskID} ({def?.title})");

        if (sequentialMode)
        {
            int next = activeTaskIndex + 1;
            if (next < tasks.Count)
            {
                activeTaskIndex = next;
                ActivateTaskAtIndex(activeTaskIndex);
            }
            else
            {
                Debug.Log("[TaskManager] All sequential tasks completed.");
                OnAllTasksCompleted?.Invoke();
            }
        }
    }

    private void ActivateTaskAtIndex(int index)
    {
        if (index < 0 || index >= tasks.Count) return;
        var t = tasks[index];
        if (t == null) return;
        Debug.Log($"[TaskManager] Activating task {t.taskID} - {t.title}");
        OnTaskActivated?.Invoke(t.taskID, t.title, Mathf.Max(1, t.targetCount));
        int cur = GetProgress(t.taskID);
        OnTaskProgressUpdated?.Invoke(t.taskID, cur, Mathf.Max(1, t.targetCount));
    }

    private void ExecuteCompletionActions(TaskDefinition def)
    {
        if (def == null) return;

        if (def.activateOnComplete != null)
        {
            foreach (var go in def.activateOnComplete)
                if (go != null) go.SetActive(true);
        }
        if (def.deactivateOnComplete != null)
        {
            foreach (var go in def.deactivateOnComplete)
                if (go != null) go.SetActive(false);
        }

        if (sceneBindings != null)
        {
            var binding = sceneBindings.Find(b => b != null && b.taskID == def.taskID);
            if (binding != null)
            {
                if (binding.activateOnComplete != null)
                    foreach (var go in binding.activateOnComplete)
                        if (go != null) go.SetActive(true);

                if (binding.deactivateOnComplete != null)
                    foreach (var go in binding.deactivateOnComplete)
                        if (go != null) go.SetActive(false);
            }
        }

        if (def.onCompleteEvent != null)
            def.onCompleteEvent.Invoke();

        if (def.loadSceneOnComplete && !string.IsNullOrEmpty(def.sceneToLoad))
            SceneManager.LoadScene(def.sceneToLoad);
    }


    public void NotifyLetterDelivered(LetterData letter, int recipientHouseID)
    {
        if (letter == null) return;

        HashSet<string> incrementedThisCall = new HashSet<string>();

        for (int i = 0; i < tasks.Count; i++)
        {
            var t = tasks[i];
            if (t == null) continue;
            if (t.taskType != TaskType.DeliverLetters) continue;
            if (completed.Contains(t.taskID)) continue;

            if (sequentialMode)
            {
                if (i != activeTaskIndex) continue;
            }

            if (t.useHouseID && t.targetHouseID != recipientHouseID) continue;
            if (incrementedThisCall.Contains(t.taskID)) continue;

            IncrementTask(t.taskID, 1);
            incrementedThisCall.Add(t.taskID);
        }
    }

    public void NotifyTalkedTo(string npcName, int houseID = -9999)
    {
        if (string.IsNullOrEmpty(npcName) && houseID == -9999) return;

        HashSet<string> incrementedThisCall = new HashSet<string>();

        for (int i = 0; i < tasks.Count; i++)
        {
            var t = tasks[i];
            if (t == null) continue;
            if (t.taskType != TaskType.TalkToNPC) continue;
            if (completed.Contains(t.taskID)) continue;

            if (sequentialMode)
            {
                if (i != activeTaskIndex) continue;
            }

            bool match = false;
            if (t.useHouseID && houseID != -9999) match = (t.targetHouseID == houseID);
            else if (!string.IsNullOrEmpty(t.targetNPCName)) match = (t.targetNPCName == npcName);
            else match = true;

            if (!match) continue;
            if (incrementedThisCall.Contains(t.taskID)) continue;

            IncrementTask(t.taskID, 1);
            incrementedThisCall.Add(t.taskID);
        }
    }

    public int GetProgress(string taskID) => progress.ContainsKey(taskID) ? progress[taskID] : 0;
    public bool IsCompleted(string taskID) => completed.Contains(taskID);
}

[System.Serializable]
public class TaskSceneBinding
{
    [Tooltip("Must match TaskDefinition.taskID exactly")]
    public string taskID;
    public GameObject[] activateOnComplete;
    public GameObject[] deactivateOnComplete;
}

