using UnityEngine;
using UnityEngine.InputSystem;

public sealed class RoundDisplay : MonoBehaviour
{
    private ElevatorRound round;
    private Font font;
    private GUIStyle caption, button;
    private InputAction point, click;
    private Rect RetryButton => new Rect(Screen.width * .5f - 76, Screen.height * .5f + 3, 152, 42);

    private void Awake()
    {
        round = GetComponent<ElevatorRound>();
        point = InputSystem.actions.FindAction("UI/Point", true);
        click = InputSystem.actions.FindAction("UI/Click", true);
        font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Apple SD Gothic Neo", "Noto Sans CJK KR" }, 24);
    }

    private void OnDestroy() => Destroy(font);

    private void Update()
    {
        if (round.Result == CallResult.Waiting || !click.WasPressedThisFrame()) return;
        Vector2 pointer = point.ReadValue<Vector2>();
        pointer.y = Screen.height - pointer.y;
        if (RetryButton.Contains(pointer)) round.Restart();
    }

    private void OnGUI()
    {
        caption ??= new GUIStyle(GUI.skin.box) { font = font, fontSize = 22, alignment = TextAnchor.MiddleCenter };
        button ??= new GUIStyle(GUI.skin.button) { font = font, fontSize = 19 };
        float x = Screen.width * .5f;
        float y = Screen.height * .5f;
        GUI.Box(new Rect(x - 185, 18, 370, 44), round.FloorGuide, caption);
        if (round.Result == CallResult.Waiting)
        {
            GUI.DrawTexture(new Rect(x - 2, y - 2, 4, 4), Texture2D.whiteTexture);
            return;
        }

        string message = round.Result switch {
            CallResult.Boarded => "탑승 성공",
            CallResult.WrongDirection => "반대 방향입니다",
            _ => "엘리베이터를 놓쳤습니다"
        };
        GUI.Box(new Rect(x - 154, y - 60, 308, 54), message, caption);
        GUI.Box(RetryButton, "다시 시도", button);
    }
}
