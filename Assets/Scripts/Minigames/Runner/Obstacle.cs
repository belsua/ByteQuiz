using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 2f;
    public bool isMovingRight = true;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 direction = isMovingRight ? Vector2.right : Vector2.left;
        rb.velocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) isMovingRight = !isMovingRight;
    }
}
