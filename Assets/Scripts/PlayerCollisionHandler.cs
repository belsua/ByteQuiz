using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            Rigidbody2D otherRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (otherRb != null)
            {
                otherRb.velocity = Vector2.zero;
                otherRb.angularVelocity = 0f;
            }
        }
    }
}
