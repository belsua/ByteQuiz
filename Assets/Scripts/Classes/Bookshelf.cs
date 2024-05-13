using UnityEngine;
using UnityEngine.UI;

public class Bookshelf : MonoBehaviour
{
    public GameObject target;
    public Button button;
    private bool collided = false;
    private bool pressed = false;

    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collided = true;
        pressed = false;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collided = false;
        CheckActivation();
    }

    private void OnButtonClick()
    {
        pressed = !pressed;
        CheckActivation();
    }

    private void CheckActivation()
    {
        if (pressed && collided) target.SetActive(true);
        else target.SetActive(false);
    }
}
