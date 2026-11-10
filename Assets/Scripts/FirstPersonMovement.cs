using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class FirstPersonMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2.6f;
    [SerializeField] private float sprintSpeed = 5.4f;
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField, Min(.1f)] private float boardingSeconds = 1.5f;

    private CharacterController controller;
    private Transform view;
    private InputAction move;
    private InputAction look;
    private InputAction sprint;
    private float pitch;
    private float verticalSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>().transform;
        var player = InputSystem.actions.FindActionMap("Player", true);
        move = player.FindAction("Move", true);
        look = player.FindAction("Look", true);
        sprint = player.FindAction("Sprint", true);
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlaceAt(Transform point, Vector3 localOffset = default)
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(point.TransformPoint(localOffset), point.rotation);
        pitch = 0f;
        verticalSpeed = 0f;
        view.localRotation = Quaternion.identity;
        controller.enabled = true;
    }

    public IEnumerator Board(Vector3 entrance, Transform cabin)
    {
        controller.enabled = false;
        Vector3 start = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion startPitch = view.localRotation;
        Quaternion facingIn = Quaternion.LookRotation(-cabin.forward);
        // Align outside the door frame before crossing the threshold.
        float elapsed = 0f;
        while (elapsed < boardingSeconds)
        {
            yield return null;
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / boardingSeconds);
            float approach = Mathf.SmoothStep(0f, 1f, progress / .45f);
            float enter = Mathf.SmoothStep(0f, 1f, (progress - .45f) / .55f);
            Vector3 position = Vector3.Lerp(Vector3.Lerp(start, entrance, approach), cabin.position, enter);
            Quaternion heading = Quaternion.Slerp(startRotation, facingIn, approach);
            float turn = Mathf.SmoothStep(0f, 1f, (progress - .68f) / .32f);
            transform.SetPositionAndRotation(position, Quaternion.Slerp(heading, cabin.rotation, turn));
            view.localRotation = Quaternion.Slerp(startPitch, Quaternion.identity, approach);
        }
        PlaceAt(cabin);
    }

    private void Update()
    {
        var aim = look.ReadValue<Vector2>();
        float sensitivity = look.activeControl?.device is Pointer ? mouseSensitivity : 120f * Time.deltaTime;
        transform.Rotate(0f, aim.x * sensitivity, 0f);
        pitch = Mathf.Clamp(pitch - aim.y * sensitivity, -85f, 85f);
        view.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        var input = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);
        float speed = sprint.IsPressed() ? sprintSpeed : walkSpeed;
        var velocity = (transform.right * input.x + transform.forward * input.y) * speed;
        if (controller.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -1f;
        verticalSpeed += Physics.gravity.y * Time.deltaTime;
        velocity.y = verticalSpeed;
        controller.Move(velocity * Time.deltaTime);
    }
}
