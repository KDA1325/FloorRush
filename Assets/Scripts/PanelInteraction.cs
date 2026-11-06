using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PanelInteraction : MonoBehaviour
{
    private InputAction interact;
    private void Awake() => interact = InputSystem.actions.FindAction("Player/Interact", true);

    private void LateUpdate()
    {
        if (ElevatorRound.Instance.Result != CallResult.Waiting || !interact.WasPressedThisFrame()) return;
        if (Physics.Raycast(transform.position, transform.forward, out var hit, 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && hit.collider.TryGetComponent<HallButton>(out var button))
            button.Press();
    }
}
