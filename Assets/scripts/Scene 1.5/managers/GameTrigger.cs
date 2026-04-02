using UnityEngine;
public class GameTrigger : MonoBehaviour
{
    public enum TriggerAction
    {
        AddTask, CompleteTask,
        OpenDoor, CloseDoor, CloseAndLockDoor,
        StartAlarm, StopAlarm,
        EnableObject, DisableObject,
        NotifyPlayerGoesDown, NotifyPlayerComesUp, NotifyPlayerEntersRoom
    }
    public TriggerAction action;
    public bool triggerOnce = true;
    public string playerTag = "Player";
    [TextArea] public string taskText;
    public GameObject targetObject;
    public AlarmSystem alarmSystem;
    public GameFlowManager gameFlowManager;
    public AudioSource audioSource;
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
            case TriggerAction.AddTask: TaskManager.Instance?.AddTask(taskText); break;
            case TriggerAction.CompleteTask: TaskManager.Instance?.CompleteTask(taskText); break;
            case TriggerAction.OpenDoor: targetObject?.GetComponent<DoorController>()?.OpenDoor(); break;
            case TriggerAction.CloseDoor: targetObject?.GetComponent<DoorController>()?.CloseDoor(); break;
            case TriggerAction.CloseAndLockDoor: var dc = targetObject?.GetComponent<DoorController>(); if (dc != null) { dc.CloseDoor(); dc.Lock(); } break;
            case TriggerAction.StartAlarm: alarmSystem?.StartAlarm(); break;
            case TriggerAction.StopAlarm: alarmSystem?.StopAlarm(); break;
            case TriggerAction.EnableObject: if (targetObject != null) targetObject.SetActive(true); break;
            case TriggerAction.DisableObject: if (targetObject != null) targetObject.SetActive(false); break;
            case TriggerAction.NotifyPlayerGoesDown: gameFlowManager?.OnPlayerGoesDown(); break;
            case TriggerAction.NotifyPlayerComesUp: gameFlowManager?.OnPlayerComesBackUp(); break;
            case TriggerAction.NotifyPlayerEntersRoom: gameFlowManager?.OnPlayerEntersAlarmRoom(); break;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        BoxCollider box = GetComponent<BoxCollider>(); if (box == null) return;
        Matrix4x4 old = Gizmos.matrix; Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size); Gizmos.matrix = old;
    }
}