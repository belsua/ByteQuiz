using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float MoveSpeed = 5f;
    public Joystick Joystick;
    private Vector2 Movement;
    private Rigidbody2D Rigidbody;
    private Animator Animator;
    public GameObject Camera;

    PhotonView PhotonView;

    private void Awake()
    {
        Joystick = FindObjectOfType<Joystick>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        PhotonView = GetComponent<PhotonView>();

        if (PhotonView.IsMine) Camera.SetActive(true);
    }

    private void Update()
    {
        if (PhotonView.IsMine) PlayerInput();
    }

    private void FixedUpdate()
    {
        if (PhotonView.IsMine) Move();
    }

    private void PlayerInput()
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

    private void Move()
    {
        Rigidbody.MovePosition(Rigidbody.position + MoveSpeed * Time.fixedDeltaTime * Movement);
    }
}
