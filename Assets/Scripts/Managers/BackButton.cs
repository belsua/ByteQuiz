using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public void ChangeScene(int i) { SceneManager.LoadScene(i); }
}
