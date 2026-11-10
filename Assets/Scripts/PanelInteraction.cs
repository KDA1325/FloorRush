using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PanelInteraction : MonoBehaviour
{
    private InputAction interact;
    public HallButton Target { get; private set; }
    private void Awake() => interact = InputSystem.actions.FindAction("Player/Interact", true);

    private void LateUpdate()
    {
        Target = null;
        if (ElevatorRound.Instance.Result == CallResult.Waiting
            && Physics.Raycast(transform.position, transform.forward, out var hit, 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && hit.collider.TryGetComponent<HallButton>(out var button))
        {
            Target = button;
            if (interact.WasPressedThisFrame()) button.Press();
        }
    }
}
