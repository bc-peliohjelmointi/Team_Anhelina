using UnityEngine;
// invisible trigger box that fires events when player walks through it
// create an empty GameObject, add BoxCollider with Is Trigger checked
// then pick an action from the dropdown in inspector
// yellow box visible in scene view so you can see where it is
public class GameTrigger : MonoBehaviour
{
    public enum TriggerAction
    {
        AddTask,
        CompleteTask,
        OpenDoor,
        CloseDoor,
        CloseAndLockDoor,
        StartAlarm,
        StopAlarm,
        EnableObject,
        DisableObject,
        // these 3 are the main ones used for the game flow sequence
        NotifyPlayerGoesDown,
        NotifyPlayerComesUp,
        NotifyPlayerEntersRoom
    }
    public TriggerAction action;
    // set false if you want trigger to fire every time player enters
    public bool triggerOnce = true;
    public string playerTag = "Player";
    // text for AddTask and CompleteTask actions, must match exactly
    [TextArea] public string taskText;
    // target object for door and enable/disable actions
    public GameObject targetObject;
    // alarm system for StartAlarm and StopAlarm actions
    public AlarmSystem alarmSystem;
    // game flow manager for the NotifyPlayer actions
    public GameFlowManager gameFlowManager;
    public AudioSource audioSource;
    // optional sound when trigger fires
    public AudioClip triggerSound;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;
        hasTriggered = true;
        if (triggerSound != null && audioSource != null) audioSource.PlayOneShot(triggerSound);

        switch (action)
        {
            case TriggerAction.AddTask:
                if (TaskManager.Instance != null) TaskManager.Instance.AddTask(taskText);
                break;
            case TriggerAction.CompleteTask:
                if (TaskManager.Instance != null) TaskManager.Instance.CompleteTask(taskText);
                break;
            case TriggerAction.OpenDoor:
                targetObject?.GetComponent<DoorController>()?.OpenDoor();
                break;
            case TriggerAction.CloseDoor:
                targetObject?.GetComponent<DoorController>()?.CloseDoor();
                break;
            case TriggerAction.CloseAndLockDoor:
                DoorController dc = targetObject?.GetComponent<DoorController>();
                if (dc != null) { dc.CloseDoor(); dc.Lock(); }
                break;
            case TriggerAction.StartAlarm:
                if (alarmSystem != null) alarmSystem.StartAlarm();
                break;
            case TriggerAction.StopAlarm:
                if (alarmSystem != null) alarmSystem.StopAlarm();
                break;
            case TriggerAction.EnableObject:
                if (targetObject != null) targetObject.SetActive(true);
                break;
            case TriggerAction.DisableObject:
                if (targetObject != null) targetObject.SetActive(false);
                break;
            case TriggerAction.NotifyPlayerGoesDown:
                if (gameFlowManager != null) gameFlowManager.OnPlayerGoesDown();
                break;
            case TriggerAction.NotifyPlayerComesUp:
                if (gameFlowManager != null) gameFlowManager.OnPlayerComesBackUp();
                break;
            case TriggerAction.NotifyPlayerEntersRoom:
                if (gameFlowManager != null) gameFlowManager.OnPlayerEntersAlarmRoom();
                break;
        }
    }

    // transparent yellow box shows in scene view so you can see the trigger area
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.matrix = old;
    }
}