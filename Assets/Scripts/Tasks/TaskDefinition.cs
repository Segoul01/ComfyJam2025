using UnityEngine;
using UnityEngine.Events;

public enum TaskType
{
    DeliverLetters,
    TalkToNPC,
    Custom
}

[CreateAssetMenu(fileName = "Task_", menuName = "Game/Task Definition")]
public class TaskDefinition : ScriptableObject
{
    [Tooltip("Unique id used by TaskManager and TaskUI (match exact)")]
    public string taskID;

    public string title;
    [TextArea] public string description;

    public TaskType taskType = TaskType.Custom;
    public int targetCount = 1;

    public bool useHouseID = false;
    public int targetHouseID = -1;

    public string targetNPCName;

    [Header("Completion actions (will run once when this task completes)")]
    public GameObject[] activateOnComplete;
    public GameObject[] deactivateOnComplete;

    [Tooltip("If set, this scene name will be loaded when the task completes. Make sure it's in Build Settings.")]
    public string sceneToLoad;
    public bool loadSceneOnComplete = false;

    public UnityEvent onCompleteEvent; // custom inspector-assignable actions
}
