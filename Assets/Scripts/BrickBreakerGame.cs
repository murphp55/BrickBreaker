using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BrickBreakerGame : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<BrickBreakerGame>() != null)
        {
            return;
        }

        var gameObject = new GameObject("BrickBreakerGame");
        gameObject.AddComponent<BrickBreakerGame>();
    }

    [Header("Layout")]
    public int columns = 10;
    public int rows = 5;
    public Vector2 brickSize = new Vector2(1.2f, 0.5f);
    public Vector2 brickPadding = new Vector2(0.15f, 0.12f);
    public float topMargin = 1.0f;
    public float sideMargin = 0.6f;

    [Header("Gameplay")]
    public int startingLives = 3;
    public float paddleWidth = 2.4f;
    public float paddleHeight = 0.35f;
    public float ballRadius = 0.22f;
    public float ballSpeed = 7.5f;

    private readonly List<Brick> _bricks = new List<Brick>();
    private PaddleController _paddle;
    private BallController _ball;
    private int _score;
    private int _lives;
    private Text _hudText;
    private PhysicsMaterial2D _bounceMaterial;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        if (_camera != null)
        {
            _camera.orthographic = true;
        }
    }

    private void Start()
    {
        _lives = startingLives;
        BuildBoard();
        UpdateHud();
    }

    private void BuildBoard()
    {
        _bounceMaterial = new PhysicsMaterial2D("Bounce");
        _bounceMaterial.bounciness = 1f;
        _bounceMaterial.friction = 0f;

        CreateHud();
        CreateWalls();
        CreatePaddle();
        CreateBall();
        CreateBricks();
    }

    private void CreateHud()
    {
        var canvasGO = new GameObject("HUD");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var textGO = new GameObject("HUDText");
        textGO.transform.SetParent(canvasGO.transform, false);
        _hudText = textGO.AddComponent<Text>();
        _hudText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _hudText.alignment = TextAnchor.UpperLeft;
        _hudText.fontSize = 24;
        _hudText.color = Color.white;

        var rect = _hudText.rectTransform;
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(16, -16);
        rect.sizeDelta = new Vector2(600, 80);
    }

    private void CreateWalls()
    {
        if (_camera == null)
        {
            return;
        }

        float height = _camera.orthographicSize * 2f;
        float width = height * _camera.aspect;
        float thickness = 0.5f;

        CreateWall("WallLeft", new Vector2(-width * 0.5f - thickness * 0.5f, 0f), new Vector2(thickness, height + 2f));
        CreateWall("WallRight", new Vector2(width * 0.5f + thickness * 0.5f, 0f), new Vector2(thickness, height + 2f));
        CreateWall("WallTop", new Vector2(0f, height * 0.5f + thickness * 0.5f), new Vector2(width + 2f, thickness));

        var deathZone = new GameObject("DeathZone");
        deathZone.transform.position = new Vector3(0f, -height * 0.5f - thickness * 0.5f, 0f);
        var collider = deathZone.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(width + 4f, thickness);
    }

    private void CreateWall(string name, Vector2 position, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.position = position;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
        collider.sharedMaterial = _bounceMaterial;
    }

    private void CreatePaddle()
    {
        var paddle = new GameObject("Paddle");
        var renderer = paddle.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSprite(Color.white);
        renderer.color = new Color(0.35f, 0.85f, 0.95f, 1f);

        var collider = paddle.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = _bounceMaterial;

        _paddle = paddle.AddComponent<PaddleController>();
        _paddle.game = this;
        _paddle.size = new Vector2(paddleWidth, paddleHeight);
        _paddle.cameraRef = _camera;

        Vector3 start = new Vector3(0f, -3.6f, 0f);
        paddle.transform.position = start;
        paddle.transform.localScale = new Vector3(paddleWidth, paddleHeight, 1f);
    }

    private void CreateBall()
    {
        var ball = new GameObject("Ball");
        var renderer = ball.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSprite(Color.white);
        renderer.color = new Color(1f, 0.85f, 0.3f, 1f);

        var collider = ball.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.sharedMaterial = _bounceMaterial;

        var rb = ball.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        _ball = ball.AddComponent<BallController>();
        _ball.game = this;
        _ball.paddle = _paddle;
        _ball.rb = rb;
        _ball.speed = ballSpeed;

        ball.transform.localScale = new Vector3(ballRadius * 2f, ballRadius * 2f, 1f);
        _ball.ResetToPaddle();
    }

    private void CreateBricks()
    {
        if (_camera == null)
        {
            return;
        }

        float height = _camera.orthographicSize * 2f;
        float width = height * _camera.aspect;
        float totalWidth = columns * brickSize.x + (columns - 1) * brickPadding.x;
        float startX = -width * 0.5f + sideMargin + brickSize.x * 0.5f;

        if (totalWidth + sideMargin * 2f < width)
        {
            startX = -totalWidth * 0.5f + brickSize.x * 0.5f;
        }

        float startY = height * 0.5f - topMargin - brickSize.y * 0.5f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 position = new Vector2(
                    startX + col * (brickSize.x + brickPadding.x),
                    startY - row * (brickSize.y + brickPadding.y));

                var brickGO = new GameObject($"Brick_{row}_{col}");
                var renderer = brickGO.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateSprite(Color.white);
                renderer.color = Color.HSVToRGB((row * 0.12f + col * 0.03f) % 1f, 0.7f, 0.95f);

                var collider = brickGO.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
                collider.sharedMaterial = _bounceMaterial;

                brickGO.transform.position = position;
                brickGO.transform.localScale = new Vector3(brickSize.x, brickSize.y, 1f);

                var brick = brickGO.AddComponent<Brick>();
                brick.game = this;
                _bricks.Add(brick);
            }
        }
    }

    private Sprite CreateSprite(Color color)
    {
        var texture = Texture2D.whiteTexture;
        var rect = new Rect(0f, 0f, texture.width, texture.height);
        var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public void OnBrickDestroyed(Brick brick)
    {
        if (brick != null)
        {
            _bricks.Remove(brick);
        }

        _score += 100;
        UpdateHud();

        if (_bricks.Count == 0)
        {
            WinGame();
        }
    }

    public void OnBallLost()
    {
        _lives -= 1;
        UpdateHud();

        if (_lives <= 0)
        {
            LoseGame();
            return;
        }

        _ball.ResetToPaddle();
    }

    private void WinGame()
    {
        _hudText.text = $"You Win!  Score: {_score}\nPress R to Restart";
        _ball.Freeze();
    }

    private void LoseGame()
    {
        _hudText.text = $"Game Over  Score: {_score}\nPress R to Restart";
        _ball.Freeze();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void UpdateHud()
    {
        if (_hudText == null)
        {
            return;
        }

        _hudText.text = $"Score: {_score}\nLives: {_lives}\nPress Space/Click to Launch";
    }
}
