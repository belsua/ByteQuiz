using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerControllerMultiplayer : PlayerController
{
    public bool isFrozen = false;
    public GameObject cameraObject;
    public TextMeshProUGUI nameText;
    PhotonView view;

    public override void Awake()
    {
        base.Awake();
        view = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (view.IsMine) cameraObject.SetActive(true);
        if (view.Controller != null) nameText.text = view.Controller.NickName;
    }

    public override void Update()
    {
        if (view.IsMine && !isFrozen) PlayerInput();
    }

    public override void FixedUpdate()
    {
        if (view.IsMine && !isFrozen) Move();
    }

    public override void OnLeftRoom() {
        if (view.IsMine) PhotonNetwork.Destroy(gameObject);
    }
}
