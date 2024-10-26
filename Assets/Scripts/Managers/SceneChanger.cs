using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void SceneChange(int index)
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        SceneManager.LoadScene(index);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
