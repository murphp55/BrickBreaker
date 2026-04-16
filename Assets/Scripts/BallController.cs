using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    public BrickBreakerGame game;
    public PaddleController paddle;
    public Rigidbody2D rb;
    public float speed = 7.5f;

    private bool _launched;
    private Vector3 _offset;

    private void Start()
    {
        _offset = new Vector3(0f, 0.5f, 0f);
    }

    private void Update()
    {
        if (!_launched)
        {
            StickToPaddle();

            bool launchPressed = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                launchPressed = true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                launchPressed = true;
            }

            if (launchPressed)
            {
                Launch();
            }
        }
        else
        {
            MaintainSpeed();
        }
    }

    public void ResetToPaddle()
    {
        _launched = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        StickToPaddle();
    }

    public void Freeze()
    {
        _launched = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void StickToPaddle()
    {
        if (paddle == null)
        {
            return;
        }
        transform.position = paddle.transform.position + _offset;
    }

    private void Launch()
    {
        _launched = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        Vector2 direction = new Vector2(Random.Range(-0.6f, 0.6f), 1f).normalized;
        rb.linearVelocity = direction * speed;
    }

    private void MaintainSpeed()
    {
        if (rb == null)
        {
            return;
        }

        if (rb.linearVelocity.sqrMagnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.up * speed;
            return;
        }

        rb.linearVelocity = rb.linearVelocity.normalized * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_launched || rb == null)
        {
            return;
        }

        Vector2 velocity = rb.linearVelocity.normalized;
        if (Mathf.Abs(velocity.y) < 0.2f)
        {
            velocity.y = 0.3f * Mathf.Sign(velocity.y == 0 ? 1f : velocity.y);
            velocity = velocity.normalized;
            rb.linearVelocity = velocity * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.name == "DeathZone")
        {
            game.OnBallLost();
        }
    }
}
