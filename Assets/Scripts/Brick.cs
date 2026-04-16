using UnityEngine;

public class Brick : MonoBehaviour
{
    public BrickBreakerGame game;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (game != null)
        {
            game.OnBrickDestroyed(this);
        }
        Destroy(gameObject);
    }
}
