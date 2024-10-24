using UnityEngine;
using UnityEngine.UI;

public class BookshelfChecker : MonoBehaviour
{
    public GameObject target;
    public Button button;
    private bool collided = false;

    public enum Subject { NumberSystem, IntroProgramming, HistoryOfComputer, ElementsOfComputer };
    public Subject bookshelfSubject;

    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        UpdateButtonInteractable();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            UpdateButtonInteractable();

        collided = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        button.interactable = false;
        collided = false;
        CheckActivation();
    }

    private void OnButtonClick()
    {
        if (button.interactable) CheckActivation();
    }

    private void CheckActivation()
    {
        if (collided) 
        {
            AudioManager.PlaySound(Resources.Load<AudioClip>("Audio/Sound/pulling-book-from-bookshelf"));
            target.SetActive(true); 
        }
        // else target.SetActive(false);
        Debug.Log("Activated");
    }

    private void UpdateButtonInteractable()
    {
        switch (bookshelfSubject)
        {
            case Subject.NumberSystem:
                button.interactable = SaveManager.player.stats.isNumberSystemUnlocked;
                break;
            case Subject.IntroProgramming:
                button.interactable = SaveManager.player.stats.isIntroProgrammingUnlocked;
                break;
            case Subject.HistoryOfComputer:
                button.interactable = true;  // Always interactable for History of Computer
                break;
            case Subject.ElementsOfComputer:
                button.interactable = true;  // Always interactable for Elements of Computer
                break;
            default:
                button.interactable = false;
                break;
        }
    }
}
