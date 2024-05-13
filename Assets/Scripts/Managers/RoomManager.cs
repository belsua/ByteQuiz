using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public Button startButton;
    public GameObject prefab;
    public GameObject playerPrefab;
    public Transform parent;
    public TextMeshProUGUI room;
    //public TextMeshProUGUI host;
    public TMP_Dropdown topicDropdown;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private ExitGames.Client.Photon.Hashtable roomOptions = new();
    [SerializeField] string selectedTopic;

    private void Start()
    {
        SpawnPlayers();

        if (PhotonNetwork.IsMasterClient) startButton.interactable = true;
        //startButton.SetActive(PhotonNetwork.IsMasterClient);

        UpdateRoomDetails();
        UpdatePlayerList();

        selectedTopic = topicDropdown.options[0].text;

        if (!PhotonNetwork.IsMasterClient) topicDropdown.interactable = false;
    }

    void SpawnPlayers()
    {
        Vector2 position = new Vector2(Random.Range(minX, maxY), Random.Range(maxX, minY));
        PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    private void UpdateRoomDetails()
    {
        room.text = $"Room: {PhotonNetwork.CurrentRoom.Name}";
        //host.text = $"Master: {PhotonNetwork.IsMasterClient}";
    }

    private void UpdatePlayerList()
    {
        if (PhotonNetwork.InRoom)
        {
            Player[] players = PhotonNetwork.PlayerList;

            foreach (Transform child in parent)
                Destroy(child.gameObject);

            foreach (Player player in players)
            {
                GameObject item = Instantiate(prefab, parent);
                TextMeshProUGUI name = item.GetComponentInChildren<TextMeshProUGUI>();
                name.text = $"Player-{player.NickName}";
            }
        }
    }

    public void OnTopicSelected()
    {
        selectedTopic = topicDropdown.options[topicDropdown.value].text;
        photonView.RPC("SyncSelectedTopic", RpcTarget.All, selectedTopic);
    }

    [PunRPC]
    void SyncSelectedTopic(string topic)
    {
        selectedTopic = topic;
        // Update the dropdown value for all clients
        int dropdownValue = FindDropdownValue(topic);
        if (dropdownValue != -1)
        {
            topicDropdown.value = dropdownValue;
        }
    }

    int FindDropdownValue(string topic)
    {
        // Find the index of the topic in the dropdown options
        for (int i = 0; i < topicDropdown.options.Count; i++)
        {
            if (topicDropdown.options[i].text == topic)
            {
                return i;
            }
        }
        return -1;
    }

    public void SetSelectedTopic()
    {
        if (PhotonNetwork.InRoom)
        {
            // Create a new Hashtable to hold the custom properties
            // ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
            // customProperties.Add("SelectedTopic", selectedTopic);

            // Set the custom properties for the current room
            // PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

            //selectedTopic = (string)PhotonNetwork.CurrentRoom.CustomProperties["selectedTopic"];
            //selectedTopic = topicDropdown.options[topicDropdown.value].text;
            //Debug.Log($"Created. Topic: {selectedTopic}");
            //ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            //hash.Add("selectedTopic", selectedTopic);
            //PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            roomOptions["selectedTopic"] = selectedTopic;
            PhotonNetwork.LocalPlayer.CustomProperties = roomOptions;

        }
        else
        {
            Debug.LogWarning("Not in a room. Cannot set custom property.");
        }
    }

    public void RequestStartGame()
    {
        photonView.RPC("StartGame", RpcTarget.All);
    }

    [PunRPC]
    private void StartGame()
    {
        SetSelectedTopic();
        PhotonNetwork.LoadLevel("Runner");
    }
}
