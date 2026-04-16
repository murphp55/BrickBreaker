using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    public BrickBreakerGame game;
    public Vector2 size = new Vector2(2.4f, 0.35f);
    public float moveSpeed = 12f;
    public Camera cameraRef;

    private float _halfWidth;

    private void Awake()
    {
        _halfWidth = size.x * 0.5f;
    }

    private void Update()
    {
        if (cameraRef == null)
        {
            return;
        }

        float height = cameraRef.orthographicSize * 2f;
        float width = height * cameraRef.aspect;
        float minX = -width * 0.5f + _halfWidth + 0.2f;
        float maxX = width * 0.5f - _halfWidth - 0.2f;

        float axis = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                axis -= 1f;
            }
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                axis += 1f;
            }
        }
        Vector3 position = transform.position;

        if (Mathf.Abs(axis) > 0.01f)
        {
            position.x += axis * moveSpeed * Time.deltaTime;
        }
        else
        {
            if (Mouse.current != null)
            {
                Vector3 mouse = Mouse.current.position.ReadValue();
                Vector3 world = cameraRef.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, -cameraRef.transform.position.z));
                position.x = Mathf.Lerp(position.x, world.x, 0.35f);
            }
        }

        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;
    }
}
