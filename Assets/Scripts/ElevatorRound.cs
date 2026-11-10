using System.Collections;
using UnityEngine;

public sealed class ElevatorRound : MonoBehaviour
{
    public static ElevatorRound Instance { get; private set; }
    [System.Serializable]
    private struct Stage
    {
        public int currentFloor;
        public int destinationFloor;
        [Min(.1f)] public float departureSeconds;
        [Tooltip("스폰 기준점의 로컬 좌표로 지정하는 출발 위치")]
        public Vector3 spawnOffset;
    }

    [SerializeField] private Stage apartment = new Stage { currentFloor = 8, destinationFloor = 1, departureSeconds = 6f };
    [SerializeField] private Stage office = new Stage
    {
        currentFloor = 1, destinationFloor = 6, departureSeconds = 7f,
        spawnOffset = new Vector3(-1.25f, 0f, -3.75f)
    };
    [SerializeField, Min(.1f)] private float closingSeconds = 2f;
    [SerializeField] private Transform spawn, cabin, leftDoor, rightDoor;
    [SerializeField] private TextMesh floorSign;
    [SerializeField] private FirstPersonMovement player;
    [SerializeField] private CabinPassengers cabinPassengers;
    [SerializeField] private CorridorPedestrians corridorPedestrians;

    private Vector3 leftOpen, rightOpen;
    private CallAttempt attempt;
    private Coroutine boardingSequence;
    private float doorClosure;
    private bool boarding;
    private bool atOffice;
    private Stage CurrentStage => atOffice ? office : apartment;
    public bool IsFinalStage => atOffice;
    public CallResult Result => attempt.Result;
    public bool ResultVisible => Result != CallResult.Waiting && !boarding;
    public string FloorGuide => $"현재 위치는 {CurrentStage.currentFloor}층, 목적지는 {CurrentStage.destinationFloor}층";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        leftOpen = leftDoor.localPosition;
        rightOpen = rightDoor.localPosition;
    }

    private void Start() => Restart();
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Restart()
    {
        if (boardingSequence != null)
        {
            StopCoroutine(boardingSequence);
            boardingSequence = null;
        }
        boarding = false;
        var stage = CurrentStage;
        attempt = new CallAttempt(stage.currentFloor, stage.destinationFloor, Time.timeAsDouble + stage.departureSeconds);
        SetDoors(0f);
        cabinPassengers.Arrange();
        corridorPedestrians.Arrange();
        floorSign.text = stage.currentFloor.ToString("00");
        player.PlaceAt(spawn, stage.spawnOffset);
        player.enabled = true;
    }

    public void ContinueFromResult()
    {
        if (!ResultVisible) return;
        if (Result == CallResult.Boarded) atOffice = !atOffice;
        Restart();
    }

    private void Update()
    {
        if (Result != CallResult.Waiting) return;
        double now = Time.timeAsDouble;
        SetDoors(1f - Mathf.Clamp01((float)(attempt.Deadline - now) / Mathf.Min(closingSeconds, CurrentStage.departureSeconds)));
        Evaluate(now, null);
    }

    public void Call(bool up) => Evaluate(Time.timeAsDouble, up);

    private void Evaluate(double now, bool? up)
    {
        if (Result != CallResult.Waiting) return;
        attempt.Evaluate(now, up);
        if (Result == CallResult.Waiting) return;
        player.enabled = false;
        if (Result == CallResult.Boarded)
        {
            boarding = true;
            boardingSequence = StartCoroutine(BoardAndDepart());
        }
        else if (Result == CallResult.Missed)
        {
            SetDoors(1f);
            ShowDepartedFloor();
        }
    }

    private IEnumerator BoardAndDepart()
    {
        yield return SlideDoorsTo(0f);
        var entrance = (leftDoor.position + rightDoor.position) * .5f + cabin.forward * .7f;
        entrance.y = cabin.position.y;
        yield return player.Board(entrance, cabin);
        boarding = false;
        yield return SlideDoorsTo(1f);
        ShowDepartedFloor();
        boardingSequence = null;
    }

    private IEnumerator SlideDoorsTo(float target)
    {
        while (doorClosure != target)
        {
            yield return null;
            SetDoors(Mathf.MoveTowards(doorClosure, target, Time.deltaTime / closingSeconds));
        }
    }

    private void ShowDepartedFloor()
    {
        var stage = CurrentStage;
        floorSign.text = (stage.currentFloor + System.Math.Sign(stage.destinationFloor - stage.currentFloor)).ToString("00");
    }

    private void SetDoors(float closed)
    {
        doorClosure = closed;
        float middle = (leftOpen.x + rightOpen.x) * .5f;
        leftDoor.localPosition = Vector3.Lerp(leftOpen, new Vector3(middle - leftDoor.localScale.x * .5f, leftOpen.y, leftOpen.z), closed);
        rightDoor.localPosition = Vector3.Lerp(rightOpen, new Vector3(middle + rightDoor.localScale.x * .5f, rightOpen.y, rightOpen.z), closed);
    }
}
