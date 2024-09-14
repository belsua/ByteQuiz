using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerControllerMultiplayer : PlayerController, IPunInstantiateMagicCallback
{
    public bool isFrozen = false;
    public GameObject cameraObject;
    public TextMeshProUGUI nameText;
    Animator animator;
    PhotonView view;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
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

    // This method gets called when the player is instantiated across the network
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // Retrieve the instantiation data, which includes the animator name
        object[] instantiationData = info.photonView.InstantiationData;

        // The avatar animator name (or index) is stored as the first element
        string avatarAnimatorName = (string)instantiationData[0];

        // Load the correct AnimatorController from the Resources folder
        RuntimeAnimatorController animatorController = Resources.Load<RuntimeAnimatorController>($"Controllers/{avatarAnimatorName}");

        // Check if the animatorController was loaded successfully
        if (animatorController != null) animator.runtimeAnimatorController = animatorController;
        else Debug.LogError("AnimatorController could not be loaded from Resources for " + avatarAnimatorName);
    }

    public override void OnLeftRoom() {
        if (view.IsMine) PhotonNetwork.Destroy(gameObject);
    }
}
