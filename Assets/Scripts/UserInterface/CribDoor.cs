using UnityEngine;

public class CribDoor : MonoBehaviour
{
    [SerializeField] GameObject returnBox;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        returnBox.SetActive(true);
    }
}
