using System.Collections;
using UnityEngine;

public sealed class ElevatorRound : MonoBehaviour
{
    public static ElevatorRound Instance { get; private set; }
    [System.Serializable]
    private class Stage
    {
        public int currentFloor;
        public int destinationFloor;
        [InspectorName("건물 최고층"), Min(1)] public int topFloor = 11;
        [InspectorName("하강 시작층"), Tooltip("현재층과 같으면 문을 열고 대기합니다.")]
        public int startFloor = 11;
        [InspectorName("승강기 정원 (플레이어 포함)"), Min(1)] public int capacity = 4;
        [InspectorName("승객 스폰 인원")] public SpawnRange passengers = new SpawnRange(1, 3);
        [InspectorName("복도 보행자 스폰 인원")] public SpawnRange pedestrians = new SpawnRange(6, 6);
        [InspectorName("승강기 양옆 배치 구간")] public SpawnStrip cabinStrip = new SpawnStrip(7.65f, 8.78f, .55f);
        [InspectorName("복도 배치 구간")] public SpawnStrip corridorStrip = new SpawnStrip(-3.1f, 4.4f, 1.5f);
        [Min(.1f)] public float departureSeconds;
        [Tooltip("스폰 기준점의 로컬 좌표로 지정하는 출발 위치")]
        public Vector3 spawnOffset;

        public void Validate()
        {
            topFloor = Mathf.Max(topFloor, Mathf.Max(currentFloor, destinationFloor));
            startFloor = Mathf.Clamp(startFloor, currentFloor, topFloor);
        }
    }

    [System.Serializable]
    private struct SpawnRange
    {
        [InspectorName("최소"), Min(0)] public int min;
        [InspectorName("최대"), Min(0)] public int max;

        public SpawnRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int Pick(int available)
        {
            int upper = Mathf.Clamp(max, 0, available);
            return Random.Range(Mathf.Clamp(min, 0, upper), upper + 1);
        }
    }

    [SerializeField] private Stage apartment = new Stage { currentFloor = 8, destinationFloor = 1, departureSeconds = 6f };
    [SerializeField] private Stage office = new Stage
    {
        currentFloor = 1, destinationFloor = 6, topFloor = 6, startFloor = 1, departureSeconds = 7f,
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
    private bool Approaching => CurrentStage.startFloor > CurrentStage.currentFloor;
    private int displayedFloor;
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
    private void OnValidate()
    {
        apartment.Validate();
        office.Validate();
    }
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
        SetDoors(Approaching ? 1f : 0f);
        int passengerSlots = Mathf.Min(cabinPassengers.transform.childCount, stage.cabinStrip.Rows * 2);
        passengerSlots = Mathf.Min(passengerSlots, Mathf.Max(1, stage.capacity) - 1);
        cabinPassengers.Arrange(stage.passengers.Pick(passengerSlots), stage.cabinStrip);
        int pedestrianSlots = Mathf.Min(corridorPedestrians.transform.childCount, stage.corridorStrip.Rows * 2);
        corridorPedestrians.Arrange(stage.pedestrians.Pick(pedestrianSlots), stage.corridorStrip);
        ShowFloor(stage.startFloor);
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
        if (!Approaching)
            SetDoors(1f - Mathf.Clamp01((float)(attempt.Deadline - now) / Mathf.Min(closingSeconds, CurrentStage.departureSeconds)));
        else
            ShowApproachingFloor(now);
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
        if (Approaching)
        {
            while (Time.timeAsDouble < attempt.Deadline)
            {
                ShowApproachingFloor(Time.timeAsDouble);
                yield return null;
            }
            ShowFloor(CurrentStage.currentFloor);
        }
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
        int direction = Approaching && Result == CallResult.Missed ? -1 : System.Math.Sign(stage.destinationFloor - stage.currentFloor);
        ShowFloor(Mathf.Clamp(stage.currentFloor + direction, 1, stage.topFloor));
    }

    private void ShowApproachingFloor(double now)
    {
        double remaining = (attempt.Deadline - now) / CurrentStage.departureSeconds;
        int floorsAbove = (int)System.Math.Ceiling(remaining * (CurrentStage.startFloor - CurrentStage.currentFloor));
        ShowFloor(CurrentStage.currentFloor + floorsAbove);
    }

    private void ShowFloor(int floor)
    {
        if (displayedFloor == floor) return;
        displayedFloor = floor;
        floorSign.text = floor.ToString("00");
    }

    private void SetDoors(float closed)
    {
        doorClosure = closed;
        float middle = (leftOpen.x + rightOpen.x) * .5f;
        leftDoor.localPosition = Vector3.Lerp(leftOpen, new Vector3(middle - leftDoor.localScale.x * .5f, leftOpen.y, leftOpen.z), closed);
        rightDoor.localPosition = Vector3.Lerp(rightOpen, new Vector3(middle + rightDoor.localScale.x * .5f, rightOpen.y, rightOpen.z), closed);
    }
}
