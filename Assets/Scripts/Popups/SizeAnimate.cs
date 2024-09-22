using UnityEngine;
using UnityEngine.UI;

public class SizeAnimate : MonoBehaviour
{
    [Range(0f, 1f)]
    public float speed = 0.3f;

    void Awake()
    {
        transform.localScale = Vector2.zero;
    }

    public void Open()
    {
        transform.LeanScale(Vector2.one, speed).setEaseOutCubic();
    }

    public void Close()
    {
        transform.LeanScale(Vector2.zero, speed).setEaseOutCubic();
    }
}
