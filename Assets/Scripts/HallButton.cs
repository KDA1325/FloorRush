using UnityEngine;

public sealed class HallButton : MonoBehaviour
{
    [SerializeField] private bool up;

    public void Press() => ElevatorRound.Instance.Call(up);
}
