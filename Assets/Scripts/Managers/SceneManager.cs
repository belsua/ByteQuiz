using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //[SerializeField] bool isReturnBoxOpen = false;

    [TextArea(2, 3)]
    public string label;

    public TextMeshProUGUI labelContainer;

    //public GameObject OnScreenPanel;
    public GameObject ReturnBox;

    private void Start()
    {
        //ReturnBox = GameObject.FindGameObjectWithTag("ReturnBox");
        labelContainer.text = label;
        //ReturnBox.SetActive(false);
    }

    //private void Awake()
    //{
    //    DontDestroyOnLoad(this.gameObject);
    //}

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //if (SceneManager.GetActiveScene().buildIndex - 1 < 0) QuitGame();
            //else ToggleReturnBox(true);
            //ToggleReturnBox(!isReturnBoxOpen);
            //ToggleGameObject(OnScreenPanel);
            ToggleGameObject(ReturnBox);
        }
    }

    public void ChangeScene(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void ToggleGameObject(GameObject targetObject)
    {
        // Check if the targetObject is not null
        if (targetObject != null)
        {
            // Set the active state of the object to the opposite of its current state
            targetObject.SetActive(!targetObject.activeInHierarchy);
        }
        else
        {
            // Debug message if targetObject is null
            Debug.LogError("ToggleGameObject: targetObject is null!");
        }
    }

    //public void ToggleReturnBox(bool toggle)
    //{
    //    if (toggle)
    //    {
    //        // Turning On
    //        OnScreenPanel.SetActive(false);
    //        ReturnBox.SetActive(true);
    //    }
    //    else
    //    {
    //        // Turning Off
    //        OnScreenPanel.SetActive(true);
    //        ReturnBox.SetActive(false);
    //    }

    //    isReturnBoxOpen = toggle;

    //    //OnScreenPanel.SetActive(false);
    //    //ReturnBox.SetActive(true);

    //    //OnScreenPanel.SetActive(true);
    //    //ReturnBox.SetActive(false);
    //}

    public void QuitGame()
    {
        Debug.Log("Quits");
        Application.Quit();
    }
}
