using UnityEngine;

public sealed class HallButton : MonoBehaviour
{
    [SerializeField] private bool up;
    [SerializeField] private Material litMaterial;
    private MeshRenderer face;
    private Material unlitMaterial;

    private void Awake()
    {
        face = GetComponent<MeshRenderer>();
        unlitMaterial = face.sharedMaterial;
    }

    private void LateUpdate()
    {
        var material = ElevatorRound.Instance.PressedDirection == up ? litMaterial : unlitMaterial;
        if (face.sharedMaterial != material) face.sharedMaterial = material;
    }

    public void Press() => ElevatorRound.Instance.Call(up);
}
