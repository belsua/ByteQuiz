using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HelpManager : MonoBehaviour
{
    public Button prevButton, nextButton;
    public GameObject[] helpPages;
    private int currentPage = 0;

    public AudioSource audioSource;
    public AudioClip clickSound;
    public GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    void Start()
    {
        // Validate setup
        if (helpPages == null || helpPages.Length == 0)
        {
            Debug.LogError("Help pages are not assigned or empty!");
            return;
        }

        eventSystem = EventSystem.current;

        // Initialize pages and buttons
        for (int i = 0; i < helpPages.Length; i++)
        {
            helpPages[i].SetActive(i == currentPage);
        }

        UpdateButtonInteractability();
    }

    void Update()
    {
        // Detect touch or click outside specific buttons
        if (!IsPointerOverSpecificUI() && gameObject.activeInHierarchy)
        {
            DetectScreenTouchOrClick();
        }
    }

    private void DetectScreenTouchOrClick()
    {
        if (Input.touchCount > 0) // Mobile: Detect touch
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleScreenSide(touch.position.x);
                audioSource.PlayOneShot(clickSound);
            }
        }
        else if (Input.GetMouseButtonDown(0)) // PC: Detect left mouse click
        {
            HandleScreenSide(Input.mousePosition.x);
        }
    }

    private void HandleScreenSide(float xPosition)
    {
        float screenWidth = Screen.width;

        if (xPosition < screenWidth / 2)
        {
            PrevPage(); // Left side tapped/clicked
        }
        else
        {
            NextPage(); // Right side tapped/clicked
        }
    }

    private bool IsPointerOverSpecificUI()
    {
        // Create pointer event data
        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition // For touch, use Input.GetTouch(0).position
        };

        // Raycast using the GraphicRaycaster
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        // Check if the raycast hit the specific buttons
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == prevButton.gameObject || result.gameObject == nextButton.gameObject)
            {
                return true; // Pointer is over one of the buttons
            }
        }

        return false; // Pointer is not over the specified buttons
    }

    public void NextPage()
    {
        if (currentPage < helpPages.Length - 1)
        {
            helpPages[currentPage].SetActive(false);
            currentPage++;
            helpPages[currentPage].SetActive(true);
            UpdateButtonInteractability();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            helpPages[currentPage].SetActive(false);
            currentPage--;
            helpPages[currentPage].SetActive(true);
            UpdateButtonInteractability();
        }
    }

    private void UpdateButtonInteractability()
    {
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < helpPages.Length - 1;
    }
}
