using UnityEngine;
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    public enum GameStage { Start, GoingDown, PuzzleActive, DoorOpened, AlarmActive, AlarmOff, ExitReady }
    public GameStage currentStage = GameStage.Start;

    public DoorController officeDoor;
    public DoorController alarmRoomDoor;
    public DoorController exitDoor;
    public AlarmSystem alarmSystem;
    public GameObject puzzleObjects;
    public GameObject computerObject;

    void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

    void Start()
    {
        if (puzzleObjects != null) puzzleObjects.SetActive(false);
        if (computerObject != null) computerObject.SetActive(false);
        if (alarmRoomDoor != null) alarmRoomDoor.Lock();
        if (exitDoor != null) exitDoor.Lock();
    }

    public void OnPlayerGoesDown()
    {
        if (currentStage != GameStage.Start) return;
        currentStage = GameStage.GoingDown;
        TaskManager.Instance?.CompleteTask(TaskManager.Instance.task_GoDown);
    }

    public void OnPlayerComesBackUp()
    {
        if (currentStage != GameStage.GoingDown) return;
        currentStage = GameStage.PuzzleActive;
        if (officeDoor != null) { officeDoor.CloseDoor(); officeDoor.Lock(); }
        if (puzzleObjects != null) puzzleObjects.SetActive(true);
        TaskManager.Instance?.AddTask(TaskManager.Instance.task_SolvePuzzle);
    }

    public void OnPuzzleDoorOpened()
    {
        if (currentStage != GameStage.PuzzleActive) return;
        currentStage = GameStage.DoorOpened;
        TaskManager.Instance?.CompleteTask(TaskManager.Instance.task_SolvePuzzle);
        TaskManager.Instance?.AddTask(TaskManager.Instance.task_EnterRoom);
    }

    public void OnPlayerEntersAlarmRoom()
    {
        if (currentStage != GameStage.DoorOpened) return;
        currentStage = GameStage.AlarmActive;
        if (alarmRoomDoor != null) { alarmRoomDoor.CloseDoor(); alarmRoomDoor.Lock(); }
        if (alarmSystem != null) alarmSystem.StartAlarm();
        if (computerObject != null) computerObject.SetActive(true);
        TaskManager.Instance?.CompleteTask(TaskManager.Instance.task_EnterRoom);
        TaskManager.Instance?.AddTask(TaskManager.Instance.task_FindCode);
        TaskManager.Instance?.AddTask(TaskManager.Instance.task_TakeCard);
    }

    public void OnAlarmDeactivated()
    {
        if (currentStage != GameStage.AlarmActive) return;
        currentStage = GameStage.AlarmOff;
        if (alarmRoomDoor != null) { alarmRoomDoor.Unlock(); alarmRoomDoor.OpenDoor(); }
    }

    public void OnExitUnlocked()
    {
        if (currentStage != GameStage.AlarmOff) return;
        currentStage = GameStage.ExitReady;
        if (exitDoor != null) { exitDoor.Unlock(); exitDoor.OpenDoor(); }
    }
}