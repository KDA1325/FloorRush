using UnityEngine;

public sealed class ElevatorRound : MonoBehaviour
{
    public static ElevatorRound Instance { get; private set; }
    [SerializeField] private int currentFloor = 8;
    [SerializeField] private int destinationFloor = 1;
    [SerializeField, Min(.1f)] private float departureSeconds = 6f;
    [SerializeField, Min(.1f)] private float closingSeconds = 2f;
    [SerializeField] private Transform spawn, cabin, leftDoor, rightDoor;
    [SerializeField] private TextMesh floorSign;
    [SerializeField] private FirstPersonMovement player;

    private Vector3 leftOpen, rightOpen;
    private CallAttempt attempt;
    public CallResult Result => attempt.Result;
    public string FloorGuide => $"현재 위치는 {currentFloor}층, 목적지는 {destinationFloor}층";

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
        attempt = new CallAttempt(currentFloor, destinationFloor, Time.timeAsDouble + departureSeconds);
        SetDoors(0f);
        floorSign.text = currentFloor.ToString("00");
        player.PlaceAt(spawn);
        player.enabled = true;
    }

    private void Update()
    {
        if (Result != CallResult.Waiting) return;
        double now = Time.timeAsDouble;
        SetDoors(1f - Mathf.Clamp01((float)(attempt.Deadline - now) / Mathf.Min(closingSeconds, departureSeconds)));
        Evaluate(now, null);
    }

    public void Call(bool up) => Evaluate(Time.timeAsDouble, up);

    private void Evaluate(double now, bool? up)
    {
        if (Result != CallResult.Waiting) return;
        attempt.Evaluate(now, up);
        if (Result == CallResult.Waiting) return;
        player.enabled = false;
        if (Result == CallResult.Boarded) player.PlaceAt(cabin);
        if (Result == CallResult.Missed || Result == CallResult.Boarded)
        {
            SetDoors(1f);
            floorSign.text = (currentFloor + System.Math.Sign(destinationFloor - currentFloor)).ToString("00");
        }
    }

    private void SetDoors(float closed)
    {
        float middle = (leftOpen.x + rightOpen.x) * .5f;
        leftDoor.localPosition = Vector3.Lerp(leftOpen, new Vector3(middle - leftDoor.localScale.x * .5f, leftOpen.y, leftOpen.z), closed);
        rightDoor.localPosition = Vector3.Lerp(rightOpen, new Vector3(middle + rightDoor.localScale.x * .5f, rightOpen.y, rightOpen.z), closed);
    }
}
