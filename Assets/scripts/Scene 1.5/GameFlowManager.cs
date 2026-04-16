using UnityEngine;
// master state machine for this scene
// controls what happens at each stage of the game
// stages go in order only, you cant skip ahead
// Start -> GoingDown -> PuzzleActive -> DoorOpened -> AlarmActive -> AlarmOff -> ExitReady
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    public enum GameStage
    {
        Start,
        GoingDown,
        PuzzleActive,
        DoorOpened,
        AlarmActive,
        AlarmOff,
        ExitReady
    }

    // you can see current stage in inspector during play mode, useful for debugging
    public GameStage currentStage = GameStage.Start;

    // the office door that closes when player comes back upstairs
    public DoorController officeDoor;
    // the door to the alarm room, locks player inside
    public DoorController alarmRoomDoor;
    // the final exit door to next area
    public DoorController exitDoor;
    public AlarmSystem alarmSystem;
    // the lever puzzle parent object, inactive at scene start
    public GameObject puzzleObjects;
    // the computer in the alarm room, inactive at scene start
    public GameObject computerObject;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // hide puzzle and computer at start
        if (puzzleObjects != null) puzzleObjects.SetActive(false);
        if (computerObject != null) computerObject.SetActive(false);
        // lock these doors, they unlock at the right moments
        if (alarmRoomDoor != null) alarmRoomDoor.Lock();
        if (exitDoor != null) exitDoor.Lock();
        // office door starts unlocked, player can enter freely at first
    }

    // trigger at bottom of stairs calls this
    public void OnPlayerGoesDown()
    {
        if (currentStage != GameStage.Start) return;
        currentStage = GameStage.GoingDown;
        if (TaskManager.Instance != null)
            TaskManager.Instance.CompleteTask(TaskManager.Instance.task_GoDown);
    }

    // trigger at top of stairs (return path) calls this
    public void OnPlayerComesBackUp()
    {
        if (currentStage != GameStage.GoingDown) return;
        currentStage = GameStage.PuzzleActive;
        // close and lock office door, player must solve puzzle to get in
        if (officeDoor != null) { officeDoor.CloseDoor(); officeDoor.Lock(); }
        // show the lever puzzle
        if (puzzleObjects != null) puzzleObjects.SetActive(true);
        if (TaskManager.Instance != null)
            TaskManager.Instance.AddTask(TaskManager.Instance.task_SolvePuzzle);
    }

    // MainDoorLever calls this after the door opens
    public void OnPuzzleDoorOpened()
    {
        if (currentStage != GameStage.PuzzleActive) return;
        currentStage = GameStage.DoorOpened;
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(TaskManager.Instance.task_SolvePuzzle);
            TaskManager.Instance.AddTask(TaskManager.Instance.task_EnterRoom);
        }
    }

    // trigger just inside the alarm room calls this
    public void OnPlayerEntersAlarmRoom()
    {
        if (currentStage != GameStage.DoorOpened) return;
        currentStage = GameStage.AlarmActive;
        // lock player inside
        if (alarmRoomDoor != null) { alarmRoomDoor.CloseDoor(); alarmRoomDoor.Lock(); }
        // start the alarm
        if (alarmSystem != null) alarmSystem.StartAlarm();
        // show the computer
        if (computerObject != null) computerObject.SetActive(true);
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(TaskManager.Instance.task_EnterRoom);
            TaskManager.Instance.AddTask(TaskManager.Instance.task_FindCode);
            TaskManager.Instance.AddTask(TaskManager.Instance.task_TakeCard);
        }
    }

    // AlarmRoom KeyCardReader calls this after card swipe
    public void OnAlarmDeactivated()
    {
        if (currentStage != GameStage.AlarmActive) return;
        currentStage = GameStage.AlarmOff;
        // unlock alarm room door so player can leave
        if (alarmRoomDoor != null) { alarmRoomDoor.Unlock(); alarmRoomDoor.OpenDoor(); }
    }

    // ExitDoor KeyCardReader calls this after card swipe
    public void OnExitUnlocked()
    {
        if (currentStage != GameStage.AlarmOff) return;
        currentStage = GameStage.ExitReady;
        if (exitDoor != null) { exitDoor.Unlock(); exitDoor.OpenDoor(); }
    }
}