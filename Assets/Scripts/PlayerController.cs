using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public float MoveSpeed = 5f;
    private Vector2 Movement;
    private Joystick Joystick;
    private Rigidbody2D Rigidbody;
    private Animator Animator;

    public virtual void Awake()
    {
        Joystick = FindObjectOfType<Joystick>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
    }

    public virtual void Update()
    {
        PlayerInput();
    }

    public virtual void FixedUpdate()
    {
        Move();
    }

    public void PlayerInput()
    {
        Movement = new Vector2(Joystick.Horizontal, Joystick.Vertical);

        Animator.SetFloat("Horizontal", Movement.x);
        Animator.SetFloat("Vertical", Movement.y);
        Animator.SetFloat("Speed", Movement.sqrMagnitude);

        if (Movement != Vector2.zero)
        {
            Animator.SetFloat("LastHorizontal", Movement.x);
            Animator.SetFloat("LastVertical", Movement.y);
        }
    }

    public void Move()
    {
        Rigidbody.MovePosition(Rigidbody.position + MoveSpeed * Time.fixedDeltaTime * Movement);
    }
}
